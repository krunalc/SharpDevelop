﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.WixBinding;
using MSBuild = Microsoft.Build.BuildEngine;
using NUnit.Framework;
using WixBinding.Tests.Utils;

namespace WixBinding.Tests.Project
{
	/// <summary>
	/// Tests the initial properties set in a newly created WixProject.
	/// </summary>
	[TestFixture]
	public class CreateNewWixProjectObjectTestFixture
	{
		ProjectCreateInformation info;
		WixProject project;
		
		[TestFixtureSetUp]
		public void SetUpFixture()
		{
			WixBindingTestsHelper.InitMSBuildEngine();
			
			info = new ProjectCreateInformation();
			info.Solution = new Solution();
			info.ProjectName = "Test";
			info.OutputProjectFileName = @"C:\Projects\Test\Test.wixproj";
			info.RootNamespace = "Test";

			project = new WixProject(info);
		}
		
		[Test]
		public void Language()
		{
			Assert.AreEqual(WixLanguageBinding.LanguageName, project.Language);
		}
		
		[Test]
		public void Name()
		{
			Assert.AreEqual(info.ProjectName, project.Name);
		}
		
		[Test]
		public void OutputName()
		{
			Assert.AreEqual(info.ProjectName, project.GetEvaluatedProperty("OutputName"));
		}
		
		[Test]
		public void OutputType()
		{
			Assert.AreEqual(WixOutputType.package.ToString(), project.GetEvaluatedProperty("OutputType"));
		}
		
		[Test]
		public void Imports()
		{
			// this is machine-dependent, it's possible that additional imports are loaded
			//Assert.AreEqual(2, project.MSBuildProject.Imports.Count);
			Microsoft.Build.BuildEngine.Import[] imports = new Microsoft.Build.BuildEngine.Import[project.MSBuildProject.Imports.Count];
			project.MSBuildProject.Imports.CopyTo(imports, 0);
			
			string[] paths = new string[imports.Length];
			for (int i = 0; i < imports.Length; i++) {
				paths[i] = imports[i].ProjectPath;
			}
			
			Assert.Contains(WixProject.DefaultTargetsFile, paths);
			Assert.Contains(@"$(MSBuildBinPath)\Microsoft.Common.targets", paths);
		}
		
		[Test]
		public void WixToolPath()
		{
			Assert.AreEqual(@"$(SharpDevelopBinPath)\Tools\Wix", project.GetUnevalatedProperty("WixToolPath"));
		}
		
		[Test]
		public void WixToolPathCondition()
		{
			MSBuild.BuildProperty property = GetMSBuildProperty("WixToolPath");
			Assert.AreEqual(" '$(WixToolPath)' == '' ", property.Condition);
		}
		
		[Test]
		public void ToolPath()
		{
			Assert.AreEqual(@"$(WixToolPath)", project.GetUnevalatedProperty("ToolPath"));
		}
		
		[Test]
		public void ToolPathHasNoCondition()
		{
			MSBuild.BuildProperty property = GetMSBuildProperty("ToolPath");
			Assert.IsTrue(String.IsNullOrEmpty(property.Condition), "ToolPath should not have a condition: " + property.Condition);
		}
		
		[Test]
		public void WixMSBuildExtensionsPath()
		{
			Assert.AreEqual(@"$(SharpDevelopBinPath)\Tools\Wix", project.GetUnevalatedProperty("WixMSBuildExtensionsPath"));
		}
		
		[Test]
		public void WixMSBuildExtensionsPathCondition()
		{
			MSBuild.BuildProperty property = GetMSBuildProperty("WixMSBuildExtensionsPath");
			Assert.AreEqual(" '$(WixMSBuildExtensionsPath)' == '' ", property.Condition);
		}
		
		[Test]
		public void DebugConfiguration()
		{
			Assert.AreEqual("Debug", project.GetEvaluatedProperty("Configuration"));
		}
		
		[Test]
		public void FileName()
		{
			Assert.AreEqual(info.OutputProjectFileName, project.FileName);
		}
		
		[Test]
		public void AssemblyName()
		{
			Assert.AreEqual("Test", project.AssemblyName);
		}
		
		[Test]
		public void UnknownProperty()
		{
			Assert.IsNull(project.GetValue("UnknownMSBuildProperty"));
		}
		
		[Test]
		public void ProjectLanguageProperties()
		{
			Assert.AreEqual(LanguageProperties.None, project.LanguageProperties);
		}
		
		/// <summary>
		/// Gets the MSBuild build property with the specified name from the WixProject.
		/// </summary>
		MSBuild.BuildProperty GetMSBuildProperty(string name)
		{
			MSBuild.Project msbuildProject = project.MSBuildProject;
			foreach (MSBuild.BuildPropertyGroup g in Linq.ToList(Linq.CastTo<MSBuild.BuildPropertyGroup>(msbuildProject.PropertyGroups))) {
				if (!g.IsImported) {
					MSBuild.BuildProperty property = MSBuildInternals.GetProperty(g, name);
					if (property != null) {
						return property;
					}
				}
			}
			return null;
		}
	}
}
