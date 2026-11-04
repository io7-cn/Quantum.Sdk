"""Verify that the public package contains only the intended .NET contracts."""

import re
import sys
from pathlib import Path
from xml.etree import ElementTree
from zipfile import ZipFile

PACKAGE_ID = "Quantum.Plugin.Abstraction"
packages = list(Path(sys.argv[1]).glob("*.nupkg"))
if len(packages) != 1 or not packages[0].name.startswith(f"{PACKAGE_ID}."):
    raise ValueError(f"Expected exactly one {PACKAGE_ID} package")

with ZipFile(packages[0]) as package:
    entries = set(package.namelist())
    metadata = {"[Content_Types].xml", "_rels/.rels"}
    payload = {
        f"{PACKAGE_ID}.nuspec",
        "README.md",
        "LICENSE",
        f"lib/net10.0/{PACKAGE_ID}.dll",
    }
    actual = {name for name in entries - metadata if not name.startswith("package/services/metadata/core-properties/")}
    if actual != payload:
        raise ValueError(f"Unexpected package contents: {sorted(actual ^ payload)}")

    local_path = re.compile(rb"(?:[A-Z]:[\\/]|\\\\|/(?:home|Users|private/var|tmp|var/tmp)/)", re.I)
    dll = package.read(f"lib/net10.0/{PACKAGE_ID}.dll")
    for marker in re.finditer(rb"RSDS", dll):
        # The CodeView record stores a signature, GUID and age before the PDB path.
        pdb_path = dll[marker.start() + 24:].split(b"\0", 1)[0]
        if local_path.search(pdb_path):
            raise ValueError("The assembly contains a workstation debug path")

    manifest = ElementTree.fromstring(package.read(f"{PACKAGE_ID}.nuspec"))
    dependencies = {dependency.get("id", "") for dependency in manifest.findall(".//{*}dependency")}
    if any(dependency.startswith("Quantum.") for dependency in dependencies):
        raise ValueError(f"SDK has an unexpected Quantum dependency: {sorted(dependencies)}")

print(f"Verified {packages[0].name}")
