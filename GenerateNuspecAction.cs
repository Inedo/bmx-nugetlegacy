﻿using System.ComponentModel;
using System.IO;
using System.Xml;
using Inedo.Agents;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web;
using Inedo.Documentation;
using Inedo.Serialization;

namespace Inedo.BuildMasterExtensions.NuGet
{
    [Tag("nuget")]
    [DisplayName("Generate .nuspec File")]
    [Description("Writes a new NuGet .nuspec file suitable for use in creating a package.")]
    [Inedo.Web.CustomEditor(typeof(GenerateNuspecActionEditor))]
    [PersistFrom("Inedo.BuildMasterExtensions.NuGet.GenerateNuspecAction,NuGet")]
    public sealed class GenerateNuspecAction : AgentBasedActionBase
    {
        private const string NuspecSchema = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";

        [Persistent]
        public string OutputFileName { get; set; }

        [Persistent]
        public string Id { get; set; }
        [Persistent]
        public string Version { get; set; } = "$ReleaseNumber";
        [Persistent]
        public string Title { get; set; }
        [Persistent]
        public string[] Authors { get; set; }
        [Persistent]
        public string Description { get; set; }
        [Persistent]
        public string Summary { get; set; }
        [Persistent]
        public string Language { get; set; }
        [Persistent]
        public string ProjectUrl { get; set; }
        [Persistent]
        public string IconUrl { get; set; }
        [Persistent]
        public string LicenseUrl { get; set; }
        [Persistent]
        public string Copyright { get; set; }
        [Persistent]
        public bool RequireLicenseAcceptance { get; set; }
        [Persistent]
        public string[] Dependencies { get; set; }
        [Persistent]
        public string[] FrameworkDependencies { get; set; }
        [Persistent]
        public string Tags { get; set; }

        public override ExtendedRichDescription GetActionDescription()
        {
            return new ExtendedRichDescription(
                new RichDescription(
                    "Generate ",
                    new Hilite(this.OutputFileName),
                    " for ",
                    new Hilite(this.Id + "." + this.Version)
                )
            );
        }

        protected override void Execute()
        {
            if (string.IsNullOrEmpty(this.Id) || string.IsNullOrEmpty(this.Version) || this.Authors == null || this.Authors.Length == 0 || string.IsNullOrEmpty(this.Description))
            {
                this.LogError("Id, Version, Authors, and Description are required.");
                return;
            }

            this.LogDebug("Generating .nuspec file...");

            var buffer = new MemoryStream();
            using (var writer = XmlWriter.Create(buffer))
            {
                writer.WriteStartElement("package", NuspecSchema);
                writer.WriteAttributeString("xmlns", null, NuspecSchema);
                writer.WriteStartElement("metadata", NuspecSchema);

                writer.WriteElementString("id", NuspecSchema, this.Id);
                writer.WriteElementString("version", NuspecSchema, this.Version);
                writer.WriteElementString("authors", NuspecSchema, string.Join(", ", this.Authors));
                writer.WriteElementString("description", NuspecSchema, this.Description);

                if (!string.IsNullOrEmpty(this.Title))
                    writer.WriteElementString("title", NuspecSchema, this.Title);
                if (!string.IsNullOrEmpty(this.Summary))
                    writer.WriteElementString("summary", NuspecSchema, this.Summary);
                if (!string.IsNullOrEmpty(this.Language))
                    writer.WriteElementString("language", NuspecSchema, this.Language);
                if (!string.IsNullOrEmpty(this.ProjectUrl))
                    writer.WriteElementString("projectUrl", NuspecSchema, this.ProjectUrl);
                if (!string.IsNullOrEmpty(this.IconUrl))
                    writer.WriteElementString("iconUrl", NuspecSchema, this.IconUrl);
                if (!string.IsNullOrEmpty(this.LicenseUrl))
                {
                    writer.WriteElementString("licenseUrl", NuspecSchema, this.LicenseUrl);
                    writer.WriteElementString("requireLicenseAcceptance", NuspecSchema, this.RequireLicenseAcceptance ? "true" : "false");
                }

                if (!string.IsNullOrEmpty(this.Copyright))
                    writer.WriteElementString("copyright", NuspecSchema, this.Copyright);
                if (!string.IsNullOrEmpty(this.Tags))
                    writer.WriteElementString("tags", NuspecSchema, this.Tags);

                if (this.Dependencies != null && this.Dependencies.Length > 0)
                {
                    writer.WriteStartElement("dependencies", NuspecSchema);
                    foreach (var dependency in this.Dependencies)
                    {
                        writer.WriteStartElement("dependency", NuspecSchema);

                        var s = dependency.Split(':');
                        writer.WriteAttributeString("id", s[0]);
                        if (s.Length > 1 && !string.IsNullOrEmpty(s[1]))
                            writer.WriteAttributeString("version", s[1]);

                        writer.WriteEndElement(); //dependency
                    }

                    writer.WriteEndElement(); //dependencies
                }

                if (this.FrameworkDependencies != null && this.FrameworkDependencies.Length > 0)
                {
                    writer.WriteStartElement("frameworkAssemblies", NuspecSchema);
                    foreach (var frameworkAssembly in this.FrameworkDependencies)
                    {
                        writer.WriteStartElement("frameworkAssembly", NuspecSchema);
                        writer.WriteAttributeString("assemblyName", frameworkAssembly);
                        writer.WriteEndElement(); //frameworkAssembly
                    }

                    writer.WriteEndElement(); //frameworkAssemblies
                }

                writer.WriteEndElement(); //metadata
                writer.WriteEndElement(); //package
            }

            var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            var fileName = fileOps.GetLegacyWorkingDirectory((IGenericBuildMasterContext)this.Context, this.OutputFileName);
            this.LogInformation("Writing {0}...", fileName);
            fileOps.WriteFileBytes(fileName, buffer.ToArray());
        }
    }
}
