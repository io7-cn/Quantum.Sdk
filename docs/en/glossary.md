# SDK terminology

[简体中文](../zh-CN/glossary.md)

## SDK

Software development kit. This repository provides the types and helpers for writing Quantum plugins.

## ABI

Application binary interface. The public assembly and type identities that compiled .NET plugins share with the host.

## RPC

Remote procedure call. Quantum routes named calls and serializes the request and response between plugin runtimes.

## DI

Dependency injection. .NET plugins receive services from their own runtime container.

## iframe

A browser frame. Quantum runs each Web plugin in a sandboxed frame and exposes host services through messages.

## SemVer

Semantic Versioning. The version components and comparison rules used by both plugin SDKs.

## Host

Quantum Desktop. It loads plugins and implements the services made available by the SDKs.
