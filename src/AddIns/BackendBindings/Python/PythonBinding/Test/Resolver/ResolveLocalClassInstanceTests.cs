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
using ICSharpCode.SharpDevelop.Dom;
using NUnit.Framework;
using PythonBinding.Tests.Utils;
using UnitTesting.Tests.Utils;

namespace PythonBinding.Tests.Resolver
{
	/// <summary>
	/// Given code:
	/// 
	/// a = Class1()
	/// 
	/// Check that the type of "a" can be obtained by the resolver.
	/// </summary>
	[TestFixture]
	public class ResolveLocalClassInstanceTests
	{
		PythonResolverTestsHelper resolverHelper;
		MockClass testClass;
		
		void CreateResolver()
		{
			CreateResolver(String.Empty);
		}
		
		void CreateResolver(string code)
		{
			resolverHelper = new PythonResolverTestsHelper(code);
			
			testClass = resolverHelper.CreateClass("Test.Test1");
			resolverHelper.ProjectContent.ClassesInProjectContent.Add(testClass);			
			resolverHelper.ProjectContent.SetClassToReturnFromGetClass("Test.Test1", testClass);
		}

		[Test]
		public void Resolve_LocalVariableIsCreatedOnPreviousLine_ResolveResultVariableNameIsA()
		{
			CreateResolver();
			
			string python =
				"a = Test.Test1()\r\n" +
				"a";
			
			resolverHelper.Resolve("a", python);
			
			string name = resolverHelper.LocalResolveResult.VariableName;
			
			Assert.AreEqual("a", name);
		}
		
		[Test]
		public void Resolve_LocalVariableIsCreatedOnPreviousLine_ResolveResultResolvedTypeIsTestClass()
		{
			CreateResolver();
			
			string python =
				"a = Test.Test1()\r\n" +
				"a";
			
			resolverHelper.Resolve("a", python);
			
			IReturnType resolvedType = resolverHelper.LocalResolveResult.ResolvedType;
			IClass underlyingClass = resolvedType.GetUnderlyingClass();
			
			Assert.AreEqual(testClass, underlyingClass);
		}
		
		[Test]
		public void Resolve_LocalVariableIsReDefinedAfterLineBeingConsidered_ResolveResultResolvedTypeIsTestClass()
		{
			CreateResolver();
			
			string python =
				"a = Test.Test1()\r\n" +
				"a\r\n" +
				"a = Unknown.Unknown()\r\n";
			
			ExpressionResult expression = new ExpressionResult("a");
			expression.Region = new DomRegion(
				beginLine: 2,
				beginColumn: 0,
				endLine: 2,
				endColumn: 1);
			
			resolverHelper.Resolve(expression, python);
			
			IReturnType resolvedType = resolverHelper.LocalResolveResult.ResolvedType;
			IClass underlyingClass = resolvedType.GetUnderlyingClass();
			
			Assert.AreEqual(testClass, underlyingClass);
		}
		
		[Test]
		public void Resolve_LocalVariableIsReDefinedAfterLineBeingConsideredAndExpressionRegionEndLineIsMinusOne_ResolveResultResolvedTypeIsTestClass()
		{
			CreateResolver();
			
			string python =
				"a = Test.Test1()\r\n" +
				"a\r\n" +
				"a = Unknown.Unknown()\r\n";
			
			ExpressionResult expression = new ExpressionResult("a");
			expression.Region = new DomRegion(
				beginLine: 2,
				beginColumn: 0,
				endLine: -1,
				endColumn: 1);
			
			resolverHelper.Resolve(expression, python);
			
			IReturnType resolvedType = resolverHelper.LocalResolveResult.ResolvedType;
			IClass underlyingClass = resolvedType.GetUnderlyingClass();
			
			Assert.AreEqual(testClass, underlyingClass);
		}
		
		[Test]
		public void Resolve_LocalVariableTypeIsImported_ResolveResultResolvedTypeDeterminedFromImportedTypes()
		{
			string python =
				"from MyNamespace import MyClass\r\n" +
				"\r\n" +
				"a = MyClass()\r\n" +
				"a";
			
			CreateResolver(python);
			
			MockClass myClass = resolverHelper.CreateClass("MyNamespace.MyClass");
			resolverHelper.ProjectContent.SetClassToReturnFromGetClass("MyNamespace.MyClass", myClass);
			
			resolverHelper.Resolve("a", python);
			
			IReturnType resolvedType = resolverHelper.LocalResolveResult.ResolvedType;
			IClass underlyingClass = resolvedType.GetUnderlyingClass();
			
			Assert.AreEqual(myClass, underlyingClass);
		}
		
		[Test]
		public void Resolve_LocalVariableMethodIsCalledOnPreviousLine_ResolveResultResolvedTypeIsTestClass()
		{
			CreateResolver();
			
			string python =
				"a = Test.Test1()\r\n" +
				"a.foo()\r\n" + 
				"a";
			
			resolverHelper.Resolve("a", python);
			
			IReturnType resolvedType = resolverHelper.LocalResolveResult.ResolvedType;
			IClass underlyingClass = resolvedType.GetUnderlyingClass();
			
			Assert.AreEqual(testClass, underlyingClass);
		}
		
		[Test]
		public void Resolve_LocalVariableMethodIsCalledAfterVariableOnItsOwnOnPreviousLine_ResolveResultResolvedTypeIsTestClass()
		{
			CreateResolver();
			
			string python =
				"a = Test.Test1()\r\n" +
				"a\r\n" +
				"a.foo()\r\n";
			
			resolverHelper.Resolve("a", python);
			
			IReturnType resolvedType = resolverHelper.LocalResolveResult.ResolvedType;
			IClass underlyingClass = resolvedType.GetUnderlyingClass();
			
			Assert.AreEqual(testClass, underlyingClass);
		}
	}
}
