﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Helpers;
using Mono.Linker.Tests.Cases.Expectations.Metadata;

namespace Mono.Linker.Tests.Cases.DataFlow
{
	// Note: this test's goal is to validate that the product correctly reports unrecognized patterns
	//   - so the main validation is done by the ExpectedWarning attributes.
	[SkipKeptItemsValidation]
	[ExpectedNoWarnings]
	public class MethodReturnParameterDataFlow
	{
		public static void Main ()
		{
			var instance = new MethodReturnParameterDataFlow ();

			// Validation that assigning value to the return value is verified
			NoRequirements ();
			instance.ReturnPublicParameterlessConstructor (typeof (TestType), typeof (TestType), typeof (TestType));
			instance.ReturnPublicParameterlessConstructorFromUnknownType (null);
			instance.ReturnPublicParameterlessConstructorFromConstant ();
			instance.ReturnPublicParameterlessConstructorFromNull ();
			instance.ReturnPublicConstructorsFailure (null);
			instance.ReturnNonPublicConstructorsFailure (null);
			instance.ReturnUnknownValue ();

			// Validation that value comming from return value of a method is correctly propagated
			instance.PropagateReturnPublicParameterlessConstructor ();
			instance.PropagateReturnPublicParameterlessConstructorFromConstant ();
			instance.PropagateReturnToReturn (0);

			instance.ReturnWithRequirementsAlwaysThrows ();

			UnsupportedReturnType ();
		}

		static Type NoRequirements ()
		{
			return typeof (TestType);
		}

		[ExpectedWarning ("IL2068", nameof (ReturnPublicParameterlessConstructor))]
		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		Type ReturnPublicParameterlessConstructor (
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
			Type publicParameterlessConstructorType,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
			Type publicConstructorsType,
			[DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.NonPublicConstructors)]
			Type nonPublicConstructorsType)
		{
			switch (GetHashCode ()) {
			case 1:
				return publicParameterlessConstructorType;
			case 2:
				return publicConstructorsType;
			case 3:
				return nonPublicConstructorsType;
			case 4:
				return typeof (TestType);
			default:
				return null;
			}
		}

		[ExpectedWarning ("IL2068", nameof (ReturnPublicParameterlessConstructorFromUnknownType))]
		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		Type ReturnPublicParameterlessConstructorFromUnknownType (Type unknownType)
		{
			return unknownType;
		}

		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		Type ReturnPublicParameterlessConstructorFromConstant ()
		{
			return typeof (TestType);
		}

		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		Type ReturnPublicParameterlessConstructorFromNull ()
		{
			return null;
		}

		Type ReturnTypeWithNoRequirements ()
		{
			return null;
		}

		// Validate error message when insufficiently annotated value is returned from a method
		[ExpectedWarning ("IL2068",
				"publicParameterlessConstructorType",
				"MethodReturnParameterDataFlow.ReturnPublicConstructorsFailure",
				"MethodReturnParameterDataFlow.ReturnPublicConstructorsFailure")]
		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicConstructors)]
		Type ReturnPublicConstructorsFailure (
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
			Type publicParameterlessConstructorType)
		{
			return publicParameterlessConstructorType;
		}

		[ExpectedWarning ("IL2068", nameof (ReturnNonPublicConstructorsFailure))]
		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.NonPublicConstructors)]
		Type ReturnNonPublicConstructorsFailure (
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
			Type publicConstructorsType)
		{
			return publicConstructorsType;
		}

		// TODO: https://github.com/dotnet/linker/issues/2273
		[ExpectedWarning ("IL2063",
			nameof (MethodReturnParameterDataFlow) + "." + nameof (ReturnUnknownValue) + "()",
			ProducedBy = ProducedBy.Trimmer)]
		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicConstructors)]
		Type ReturnUnknownValue ()
		{
			var array = new object[1];
			array[0] = this.GetType ();
			MakeArrayValuesUnknown (array);
			return (Type) array[0];

			static void MakeArrayValuesUnknown (object[] array)
			{
			}
		}

		[ExpectedWarning ("IL2072",
			nameof (DataFlowTypeExtensions) + "." + nameof (DataFlowTypeExtensions.RequiresPublicConstructors) + "(Type)",
			nameof (MethodReturnParameterDataFlow) + "." + nameof (ReturnPublicParameterlessConstructor) + "(Type, Type, Type)")]
		[ExpectedWarning ("IL2072",
			nameof (DataFlowTypeExtensions) + "." + nameof (DataFlowTypeExtensions.RequiresNonPublicConstructors) + "(Type)",
			nameof (MethodReturnParameterDataFlow) + "." + nameof (ReturnPublicParameterlessConstructor) + "(Type, Type, Type)")]
		void PropagateReturnPublicParameterlessConstructor ()
		{
			Type t = ReturnPublicParameterlessConstructor (typeof (TestType), typeof (TestType), typeof (TestType));
			t.RequiresPublicParameterlessConstructor ();
			t.RequiresPublicConstructors ();
			t.RequiresNonPublicConstructors ();
			t.RequiresNone ();
		}

		[ExpectedWarning ("IL2072",
			nameof (DataFlowTypeExtensions) + "." + nameof (DataFlowTypeExtensions.RequiresPublicConstructors) + "(Type)",
			nameof (MethodReturnParameterDataFlow) + "." + nameof (ReturnPublicParameterlessConstructorFromConstant) + "()")]
		[ExpectedWarning ("IL2072",
			nameof (DataFlowTypeExtensions) + "." + nameof (DataFlowTypeExtensions.RequiresNonPublicConstructors) + "(Type)",
			nameof (MethodReturnParameterDataFlow) + "." + nameof (ReturnPublicParameterlessConstructorFromConstant) + "()")]
		void PropagateReturnPublicParameterlessConstructorFromConstant ()
		{
			Type t = ReturnPublicParameterlessConstructorFromConstant ();
			t.RequiresPublicParameterlessConstructor ();
			t.RequiresPublicConstructors ();
			t.RequiresNonPublicConstructors ();
			t.RequiresNone ();
		}

		[ExpectedWarning ("IL2073",
				nameof (ReturnTypeWithNoRequirements),
				nameof (PropagateReturnToReturn))]
		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		Type PropagateReturnToReturn (int n)
		{
			switch (n) {
			case 0:
				return ReturnPublicParameterlessConstructorFromConstant ();
			case 1:
				return ReturnTypeWithNoRequirements ();
			}

			return null;
		}

		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicMethods)]
		Type ReturnWithRequirementsAlwaysThrows ()
		{
			throw new NotImplementedException ();
		}

		// TODO: https://github.com/dotnet/linker/issues/2273
		[ExpectedWarning ("IL2106", nameof (UnsupportedReturnType), ProducedBy = ProducedBy.Trimmer)]
		[return: DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicMethods)]
		static object UnsupportedReturnType () => null;

		class TestType
		{
			public TestType () { }
		}
	}
}
