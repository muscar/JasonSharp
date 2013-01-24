using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using JasonSharp.Frontend;

namespace JasonSharp.Backend
{
	abstract class SymbolTableEntry
	{
		public abstract void EmitLookup(ILGenerator il);
	}

	class ArgumentEntry : SymbolTableEntry
	{
		public readonly int Info;
		
		public ArgumentEntry(int info)
		{
			this.Info = info;
		}

		public override void EmitLookup(ILGenerator il)
		{
			switch (Info)
			{
			case 0:
				il.Emit(OpCodes.Ldarg_0);
				break;
			case 1:
				il.Emit(OpCodes.Ldarg_1);
				break;
			case 2:
				il.Emit(OpCodes.Ldarg_2);
				break;
			case 3:
				il.Emit(OpCodes.Ldarg_3);
				break;
			default:
				il.Emit(OpCodes.Ldarg, Info);
				break;
			}
		}
	}

	class LocalEntry : SymbolTableEntry
	{
		public readonly LocalBuilder Info;

		public LocalEntry(LocalBuilder info)
		{
			this.Info = info;
		}

		public override void EmitLookup(ILGenerator il)
		{
			il.Emit(OpCodes.Ldloc, Info);
		}
	}

	class FieldEntry : SymbolTableEntry
	{
		public readonly FieldBuilder Info;

		public FieldEntry(FieldBuilder info)
		{
			this.Info = info;
		}

		public override void EmitLookup(ILGenerator il)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, Info);
		}
	}

	class MethodEntry : SymbolTableEntry
	{
		public readonly MethodBuilder Info;

		public MethodEntry(MethodBuilder info)
		{
			this.Info = info;
		}

		public override void EmitLookup(ILGenerator il)
		{
			throw new NotImplementedException();
		}
	}

	class CodeGenerator : INodeVisitor
	{
		private readonly AssemblyBuilder assemblyBuilder;
		private readonly ModuleBuilder moduleBuilder;
		private TypeBuilder typeBuilder;

		private readonly SymbolTable<SymbolTableEntry> symbolTable = new SymbolTable<SymbolTableEntry>();

		// XXX This is here only for debugging convenience
		private MethodBuilder methodBuilder;
		private ILGenerator il;

		public CodeGenerator(string moduleName)
		{
			// create a dynamic assembly and module 
			var assemblyName = new AssemblyName() { Name = moduleName };
			assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
			moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName + ".dll");
		}

		public void Bake()
		{
			// bake it
			typeBuilder.CreateType();

			// set the entry point for the application and save it
//			assemblyBuilder.SetEntryPoint(methodBuilder, PEFileKinds.Dll);
			assemblyBuilder.Save(moduleBuilder.ScopeName);
		}

        #region INodeVisitor Members

		public void Visit(AgentDeclarationNode node)
		{
			// create a new type to hold our Main method
			typeBuilder = moduleBuilder.DefineType(node.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit);

			var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.HasThis, node.Args.Select(arg => typeof(int)).ToArray());
			il = ctorBuilder.GetILGenerator();

			// Agent scope
			symbolTable.Enter();

			foreach (var b in node.BeliefDeclarations)
			{
				var field = typeBuilder.DefineField(b.Name, MakeTupleType(b.Args), FieldAttributes.Public);
				symbolTable.Register(b.Name, new FieldEntry(field));
			}

			EmitCtor(node.Args, node.BeliefDeclarations);

			foreach (var n in node.Body)
			{
				n.Accept(this);
			}

//			// create the Main(string[] args) method
//			methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { typeof(string[]) });
//			
//			// generate the IL for the Main method
//			il = methodBuilder.GetILGenerator();
//
//			il.Emit(OpCodes.Ret);

			symbolTable.Exit();
		}

		private void EmitCtor(IList<Tuple<string, string>> args, IList<BeliefDeclarationNode> beliefDeclarations)
		{
			symbolTable.Enter();
			
			for (int i = 0; i < args.Count; i++)
			{
				symbolTable.Register(args[i].Item1, new ArgumentEntry(i + 1));
			}

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
			
			foreach (var b in beliefDeclarations)
			{
				var field = symbolTable.Lookup<FieldEntry>(b.Name);
				if (field != null)
				{
					il.Emit(OpCodes.Ldarg_0);
					EmitTupleCreate(field.GetType(), b.Args);
					il.Emit(OpCodes.Stfld, field.Info);
				}
			}
			
			il.Emit(OpCodes.Ret);
			
			symbolTable.Exit();
		}

		private Type MakeTupleType(IList<Node> args)
		{
			var tupleOf = Type.GetType("System.Tuple`" + args.Count);
			var genericTypeOf = tupleOf.MakeGenericType(args.Select(a => typeof(int)).ToArray());
			return genericTypeOf;
		}

		private void EmitTupleCreate(Type genericTupleOf, IList<Node> args)
		{
			foreach (var meth in typeof(Tuple).GetMethods())
			{
				if (meth.Name == "Create" && meth.GetGenericArguments().Length == args.Count)
				{
					foreach (var arg in args)
					{
						arg.Accept(this);
					}

					var createMeth = meth.MakeGenericMethod(args.Select(a => typeof(int)).ToArray());
					
					il.Emit(OpCodes.Call, createMeth);
					return;
				}
			}
		}

		public void Visit(BeliefDeclarationNode node)
		{
			throw new NotImplementedException();
		}

		public void Visit(HandlerDeclarationNode node)
		{
			throw new NotImplementedException();
		}

		public void Visit(PlanDeclarationNode node)
		{
			methodBuilder = typeBuilder.DefineMethod(node.Name, MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), node.Args.Select(a => typeof(int)).ToArray());
			il = methodBuilder.GetILGenerator();

			symbolTable.Enter();

			var args = node.Args;
			for (int i = 0; i < args.Count; i++)
			{
				symbolTable.Register(args[i].Item1, new ArgumentEntry(i + 1));
			}

			foreach (var n in node.Body)
			{
				n.Accept(this);
			}

			il.Emit(OpCodes.Ret);
			
			symbolTable.Exit();

			symbolTable.Register(node.Name, new MethodEntry(methodBuilder));
		}

		public void Visit(BeliefQueryNode node)
		{
			var field = symbolTable.Lookup<FieldEntry>(node.Name);
			var args = node.Args;
			var genericTupleOf = MakeTupleType(args);

			for (int i = 0; i < args.Count; i++)
			{
				var local = il.DeclareLocal(typeof(int));
				symbolTable.Register((args[i] as IdentNode).Name, new LocalEntry(local));
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, field.Info);
				il.Emit(OpCodes.Call, genericTupleOf.GetMethod("get_Item" + (i + 1)));
				il.Emit(OpCodes.Stloc, local);
			}
		}

		public void Visit(BeliefUpdateNode node)
		{
			var field = symbolTable.Lookup<FieldEntry>(node.Name);
			if (field != null)
			{
				il.Emit(OpCodes.Ldarg_0);
				EmitTupleCreate(field.GetType(), node.Args);
				il.Emit(OpCodes.Stfld, field.Info);
			}
		}

		public void Visit(BinaryOpNode node)
		{
			node.Left.Accept(this);
			node.Right.Accept(this);
			il.Emit(OpCodes.Add); // XXX
		}

		public void Visit(IdentNode node)
		{
			var info = symbolTable.Lookup(node.Name);
			info.EmitLookup(il);
		}

		public void Visit(NumberNode node)
		{
			il.Emit(OpCodes.Ldc_I4, node.Value);
		}

        #endregion
	}
}
