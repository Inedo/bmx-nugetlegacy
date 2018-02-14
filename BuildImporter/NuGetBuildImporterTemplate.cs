﻿using Inedo.BuildMaster.Extensibility.BuildImporters;
using Inedo.BuildMaster.Web;
using Inedo.Documentation;
using Inedo.Serialization;

namespace Inedo.BuildMasterExtensions.NuGet.BuildImporter
{
    [Inedo.Web.CustomEditor(typeof(NuGetBuildImporterTemplateEditor))]
    [PersistFrom("Inedo.BuildMasterExtensions.NuGet.BuildImporter.NuGetBuildImporterTemplate,NuGet")]
    internal sealed class NuGetBuildImporterTemplate : BuildImporterTemplateBase<NuGetBuildImporter>
    {
        [Persistent]
        public string PackageId { get; set; }
        [Persistent]
        public string PackageVersion { get; set; }
        [Persistent]
        public string PackageSource { get; set; }
        [Persistent]
        public bool IncludePrerelease { get; set; }
        [Persistent]
        public bool VersionLocked { get; set; }
        [Persistent]
        public string AdditionalArguments { get; set; }
        [Persistent]
        public bool CaptureIdAndVersion { get; set; }
        [Persistent]
        public string PackageArtifactRoot { get; set; }
        [Persistent]
        public bool IncludeVersionInArtifactName { get; set; }

        public override RichDescription GetDescription()
        {
            var description = new RichDescription(
                "Import NuGet package ",
                new Hilite(this.PackageId)
            );

            if (!string.IsNullOrEmpty(this.PackageVersion))
                description.AppendContent(new Hilite(this.PackageVersion));

            description.AppendContent(
                " from ",
                new Hilite(!string.IsNullOrEmpty(this.PackageSource) ? this.PackageSource : "default package source")
            );

            if (this.CaptureIdAndVersion)
                description.AppendContent(" and set $ImportedPackageId and $ImportedPackageVersion build variables");

            return description;
        }
    }
}
