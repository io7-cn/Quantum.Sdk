using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace Quantum.Plugin.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class QuantumRequestAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor MissingDeclaration = Rule("QTM001",
        "Declare a Request capability", "Quantum service method '{0}' requires [RequestCapability]");
    private static readonly DiagnosticDescriptor InvalidPolicy = Rule("QTM002",
        "Use a valid Request policy", "Request '{0}' has an invalid dispatch policy");
    private static readonly DiagnosticDescriptor InvalidResult = Rule("QTM003",
        "Use a supported Request result", "Request '{0}' must return {1}");

    private static readonly DiagnosticDescriptor StopNotAllowed = Rule("QTM004",
        "Declare ordered stopping", "Request '{0}' must declare AllowStop before its handler can call AnswerAndStop");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(MissingDeclaration, InvalidPolicy, InvalidResult, StopNotAllowed);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(start =>
        {
            var transport = start.Compilation.GetTypeByMetadataName("Quantum.Plugin.Abstraction.TransportOverQuantumAttribute");
            var request = start.Compilation.GetTypeByMetadataName("Quantum.Plugin.Abstraction.RequestCapabilityAttribute");
            var requestResult = start.Compilation.GetTypeByMetadataName("Quantum.Plugin.Abstraction.QuantumRequestResult`1");
            var result = start.Compilation.GetTypeByMetadataName("NOF.Contract.Result");
            var genericResult = start.Compilation.GetTypeByMetadataName("NOF.Contract.Result`1");
            if (transport is null || request is null || requestResult is null || result is null || genericResult is null)
            {
                return;
            }

            start.RegisterSymbolAction(action =>
            {
                var method = (IMethodSymbol)action.Symbol;
                if (method.MethodKind != MethodKind.Ordinary || method.IsStatic
                    || !method.ContainingType.GetAttributes().Any(attribute => Same(attribute.AttributeClass, transport)))
                {
                    return;
                }

                var declaration = method.GetAttributes().FirstOrDefault(attribute => Same(attribute.AttributeClass, request));
                var location = method.Locations[0];
                if (declaration is null)
                {
                    action.ReportDiagnostic(Diagnostic.Create(MissingDeclaration, location, method.Name));
                    return;
                }

                if (declaration.ConstructorArguments.Length != 2
                    || declaration.ConstructorArguments[0].Value is not string name
                    || declaration.ConstructorArguments[1].Value is not int dispatch)
                {
                    return;
                }

                var uniqueOwner = IsEnabled(declaration, "UniqueOwner");
                var allowStop = IsEnabled(declaration, "AllowStop");
                if (dispatch is not (0 or 1) || dispatch == 0 && (uniqueOwner || allowStop)
                    || IsEnabled(declaration, "RequireSingleProvider") && !uniqueOwner)
                {
                    action.ReportDiagnostic(Diagnostic.Create(InvalidPolicy, location, name));
                }

                var returnType = method.ReturnType.OriginalDefinition;
                if (!Same(returnType, requestResult)
                    && (allowStop || !Same(returnType, result) && !Same(returnType, genericResult)))
                {
                    var expected = allowStop ? "QuantumRequestResult<T>" : "Result, Result<T>, or QuantumRequestResult<T>";
                    action.ReportDiagnostic(Diagnostic.Create(InvalidResult, location, name, expected));
                }
            }, SymbolKind.Method);

            var rpcServer = start.Compilation.GetTypeByMetadataName("NOF.Application.RpcServer`1");
            if (rpcServer is null)
            {
                return;
            }

            start.RegisterOperationAction(action =>
            {
                var invocation = (IInvocationOperation)action.Operation;
                if (invocation.TargetMethod.Name != "AnswerAndStop"
                    || !Same(invocation.TargetMethod.ContainingType.OriginalDefinition, requestResult)
                    || action.ContainingSymbol is not IMethodSymbol { Name: "HandleAsync", IsOverride: true } handler
                    || FindDeclaration(handler.ContainingType, rpcServer, request) is not { } declaration
                    || IsEnabled(declaration, "AllowStop"))
                {
                    return;
                }

                action.ReportDiagnostic(Diagnostic.Create(StopNotAllowed, invocation.Syntax.GetLocation(),
                    declaration.ConstructorArguments[0].Value));
            }, OperationKind.Invocation);
        });
    }

    private static AttributeData? FindDeclaration(INamedTypeSymbol handler, INamedTypeSymbol rpcServer, INamedTypeSymbol request)
    {
        for (var handlerBase = handler.BaseType; handlerBase is not null; handlerBase = handlerBase.BaseType)
        {
            for (var serverBase = handlerBase.ContainingType?.BaseType; serverBase is not null; serverBase = serverBase.BaseType)
            {
                if (Same(serverBase.OriginalDefinition, rpcServer)
                    && serverBase.TypeArguments[0] is INamedTypeSymbol contract)
                {
                    return contract.GetMembers(handlerBase.Name).OfType<IMethodSymbol>()
                        .SelectMany(method => method.GetAttributes())
                        .FirstOrDefault(attribute => Same(attribute.AttributeClass, request));
                }
            }
        }

        return null;
    }

    private static bool IsEnabled(AttributeData attribute, string name)
        => attribute.NamedArguments.Any(argument => argument.Key == name && argument.Value.Value is true);

    private static bool Same(ISymbol? left, ISymbol right) => SymbolEqualityComparer.Default.Equals(left, right);

    private static DiagnosticDescriptor Rule(string id, string title, string message)
        => new(id, title, message, "Quantum capabilities", DiagnosticSeverity.Error, isEnabledByDefault: true);
}
