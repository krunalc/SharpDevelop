﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using ICSharpCode.PythonBinding;
using ICSharpCode.Scripting.Tests.Utils;
using NUnit.Framework;
using PythonBinding.Tests.Utils;

namespace PythonBinding.Tests.Designer
{
	/// <summary>
	/// Statements before the main form class are parsed by the PythonComponentWalker when 
	/// they should be ignored. This test fixture checks that only when the InitializeComponent method
	/// is found the statements are parsed.
	/// </summary>
	[TestFixture]
	public class LoadFormWithSysPathAppendStatementTestFixture : LoadFormTestFixtureBase
	{		
		public override string PythonCode {
			get {	
				return "import sys\r\n" +
							"sys.path.append(r'c:\\python\\lib')\r\n" + // Calls Walk(CallExpression)
							"a = System.Windows.Forms.TextBox()\r\n" + // Calls Walk(AssignmentStatement)
							"a.Load += Load\r\n" + // Calls Walk(AugmentedAssignStatement)
							"b\r\n" + // Calls Walk(NameExpression)
							"10\r\n" + // Calls Walk(ConstantExpression)
							"\r\n" +
							"class MainForm(System.Windows.Forms.Form):\r\n" +
							"    def InitializeComponent(self):\r\n" +
							"        self.SuspendLayout()\r\n" +
							"        # \r\n" +
							"        # MainForm\r\n" +
							"        # \r\n" +
							"        self.Name = \"MainForm\"\r\n" +
							"        self.ResumeLayout(False)\r\n";
			}
		}
				
		public CreatedComponent FormComponent {
			get { return ComponentCreator.CreatedComponents[0]; }
		}
		
		[Test]
		public void MainFormCreated()
		{			
			Assert.IsNotNull(Form);
		}
		
		[Test]
		public void NoInstancesCreated()
		{
			Assert.AreEqual(0, ComponentCreator.CreatedInstances.Count);
		}
	}
}
