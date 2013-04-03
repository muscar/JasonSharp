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

    class CodeGeneratorState
    {
        public readonly SymbolTable<SymbolTableEntry> symbolTable = new SymbolTable<SymbolTableEntry>();

        public readonly AssemblyBuilder assemblyBuilder;
        public readonly ModuleBuilder moduleBuilder;
        public TypeBuilder typeBuilder;
        public MethodBuilder methodBuilder;
        public ILGenerator il;

        public CodeGeneratorState(AssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder)
        {
            this.assemblyBuilder = assemblyBuilder;
            this.moduleBuilder = moduleBuilder;
        }

        public FieldBuilder DefineField(string name, Type type, FieldAttributes attributes)
        {
            var field = typeBuilder.DefineField(name, type, attributes);
            symbolTable.Register(name, new FieldEntry(field));
            return field;
        }
    }

    class CodeGenerator : INodeVisitor<CodeGeneratorState>
    {
        private readonly MethodInfo[] tupleCreateMethods = new MethodInfo[8];

        public CodeGenerator()
        {
            foreach (var meth in typeof(Tuple).GetMethods())
            {
                if (meth.Name == "Create")
                {
                    tupleCreateMethods [meth.GetGenericArguments().Length - 1] = meth;
                }
            }
        }

        public static void Compile(string moduleName, Node moduleAst)
        {
            var assemblyName = new AssemblyName() { Name = moduleName };
            var assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName + ".dll");
            var state = new CodeGeneratorState(assemblyBuilder, moduleBuilder);
            var cg = new CodeGenerator();
            moduleAst.Accept(cg, state);
            // Bake it
            state.typeBuilder.CreateType();
            state.assemblyBuilder.Save(moduleBuilder.ScopeName);
        }

        #region INodeVisitor Members

        public void Visit(AgentDeclarationNode node, CodeGeneratorState state)
        {
            // create a new type to hold our Main method
            state.typeBuilder = state.moduleBuilder.DefineType(node.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit);

            // Agent scope
            state.symbolTable.Enter();

            foreach (var b in node.BeliefDeclarations)
            {
                b.Accept(this, state);
            }

            var ctorBuilder = state.typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.HasThis, node.Args.Select(arg => typeof(int)).ToArray());
            state.il = ctorBuilder.GetILGenerator();
			
            EmitCtor(state, node.Args, node.BeliefDeclarations);

            foreach (var n in node.Body)
            {
                n.Accept(this, state);
            }

//			// create the Main(string[] args) method
//			methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { typeof(string[]) });
//			
//			// generate the IL for the Main method
//			il = methodBuilder.GetILGenerator();
//
//			il.Emit(OpCodes.Ret);

            state.symbolTable.Exit();
        }

        private void EmitCtor(CodeGeneratorState state, IList<Tuple<string, string>> args, IList<BeliefDeclarationNode> beliefDeclarations)
        {
            state.symbolTable.Enter();
			
            for (int i = 0; i < args.Count; i++)
            {
                state.symbolTable.Register(args [i].Item1, new ArgumentEntry(i + 1));
            }

            state.il.Emit(OpCodes.Ldarg_0);
            state.il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
			
            foreach (var b in beliefDeclarations)
            {
                var field = state.symbolTable.Lookup<FieldEntry>(b.Name);
                if (field != null)
                {
                    state.il.Emit(OpCodes.Ldarg_0);
                    EmitTupleCreate(state, field.GetType(), b.Args);
                    state.il.Emit(OpCodes.Stfld, field.Info);
                }
            }
			
            state.il.Emit(OpCodes.Ret);
			
            state.symbolTable.Exit();
        }

        private Type MakeTupleType(IList<Node> args)
        {
            var tupleOf = Type.GetType("System.Tuple`" + args.Count);
            var genericTypeOf = tupleOf.MakeGenericType(args.Select(a => typeof(int)).ToArray());
            return genericTypeOf;
        }

        private void EmitTupleCreate(CodeGeneratorState state, Type genericTupleOf, IList<Node> args)
        {
            if (args.Count >= tupleCreateMethods.Length)
            {
                throw new ApplicationException(String.Format("Can't have more than {0} arguments. Yeah, it sucks, I know.", tupleCreateMethods.Length));
            }

            var meth = tupleCreateMethods [args.Count - 1];
            var createMeth = meth.MakeGenericMethod(args.Select(a => typeof(int)).ToArray());

            foreach (var arg in args)
            {
                arg.Accept(this, state);
            }

            state.il.Emit(OpCodes.Call, createMeth);
        }

        public void Visit(BeliefDeclarationNode node, CodeGeneratorState state)
        {
            state.DefineField(node.Name, MakeTupleType(node.Args), FieldAttributes.Public);
        }

        public void Visit(HandlerDeclarationNode node, CodeGeneratorState state)
        {
            throw new NotImplementedException();
        }

        public void Visit(PlanDeclarationNode node, CodeGeneratorState state)
        {
            state.methodBuilder = state.typeBuilder.DefineMethod(node.Name, MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), node.Args.Select(a => typeof(int)).ToArray());
            state.il = state.methodBuilder.GetILGenerator();

            state.symbolTable.Enter();

            var args = node.Args;
            for (int i = 0; i < args.Count; i++)
            {
                state.symbolTable.Register(args [i].Item1, new ArgumentEntry(i + 1));
            }

            foreach (var n in node.Body)
            {
                n.Accept(this, state);
            }

            state.il.Emit(OpCodes.Ret);
			
            state.symbolTable.Exit();

            state.symbolTable.Register(node.Name, new MethodEntry(state.methodBuilder));
        }

        public void Visit(BeliefQueryNode node, CodeGeneratorState state)
        {
            var field = state.symbolTable.Lookup<FieldEntry>(node.Name);
            var args = node.Args;
            var genericTupleOf = MakeTupleType(args);

            for (int i = 0; i < args.Count; i++)
            {
                var local = state.il.DeclareLocal(typeof(int));
                state.symbolTable.Register((args [i] as IdentNode).Name, new LocalEntry(local));
                state.il.Emit(OpCodes.Ldarg_0);
                state.il.Emit(OpCodes.Ldfld, field.Info);
                state.il.Emit(OpCodes.Call, genericTupleOf.GetMethod("get_Item" + (i + 1)));
                state.il.Emit(OpCodes.Stloc, local);
            }
        }

        public void Visit(BeliefUpdateNode node, CodeGeneratorState state)
        {
            var field = state.symbolTable.Lookup<FieldEntry>(node.Name);
            if (field != null)
            {
                state.il.Emit(OpCodes.Ldarg_0);
                EmitTupleCreate(state, field.GetType(), node.Args);
                state.il.Emit(OpCodes.Stfld, field.Info);
            }
        }

        public void Visit(BinaryOpNode node, CodeGeneratorState state)
        {
            node.Left.Accept(this, state);
            node.Right.Accept(this, state);
            state.il.Emit(OpCodes.Add); // XXX
        }

        public void Visit(IdentNode node, CodeGeneratorState state)
        {
            var info = state.symbolTable.Lookup(node.Name);
            info.EmitLookup(state.il);
        }

        public void Visit(NumberNode node, CodeGeneratorState state)
        {
            state.il.Emit(OpCodes.Ldc_I4, node.Value);
        }

        #endregion
    }
}
