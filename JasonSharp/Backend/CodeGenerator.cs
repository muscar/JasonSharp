using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

using JasonSharp.Intermediate;
using JasonSharp.Backend;

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
            il.Emit(OpCodes.Ldftn, Info);
        }
    }

    class CodeGenerator : INodeVisitor
    {
        private readonly SymbolTable<string, SymbolTableEntry> symbolTable = new SymbolTable<string, SymbolTableEntry>();

        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private TypeBuilder typeBuilder;
        private MethodBuilder methodBuilder;
        private ILGenerator il;

        public CodeGenerator(string moduleName)
        {
            var assemblyName = new AssemblyName { Name = moduleName };
            assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName + ".dll");
        }

        public void Compile(INode moduleAst)
        {
            moduleAst.Accept(this);
            // Bake it
            typeBuilder.CreateType();
            assemblyBuilder.Save(moduleBuilder.ScopeName);
        }

        private void EmitCtor(IList<Tuple<string, string>> args, IEnumerable<BeliefDeclarationNode> beliefDeclarations)
        {
            symbolTable.EnterScope();

            for (int i = 0; i < args.Count; i++)
            {
                symbolTable.Register(args[i].Item1, new ArgumentEntry(i + 1));
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

            foreach (var belief in beliefDeclarations)
            {
                EmitBeliefUpdate(belief.Name, belief.Args);
            }

            il.Emit(OpCodes.Ret);

            symbolTable.ExitScope();
        }

        void EmitBeliefUpdate(string name, IEnumerable<INode> args)
        {
            var field = symbolTable.LookupAs<FieldEntry>(name);
            if (field != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                foreach (var arg in args)
                {
                    arg.Accept(this);
                }
                il.EmitTupleCreate(args.Select(a => typeof(int)).ToArray());
                il.Emit(OpCodes.Stfld, field.Info);
            }
        }

        #region INodeVisitor Members

        public void Visit(AgentDeclarationNode node)
        {
            typeBuilder = moduleBuilder.DefineType(node.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit);

            // Agent scope
            symbolTable.EnterScope();

            foreach (var b in node.BeliefDeclarations)
            {
                b.Accept(this);
            }

            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.HasThis, node.Args.Select(arg => typeof(int)).ToArray());
            il = ctorBuilder.GetILGenerator();
			
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

            symbolTable.ExitScope();
        }

        public void Visit(BeliefDeclarationNode node)
        {
            var argTypes = node.Args.Select(a => typeof(int)).ToArray();
            var field = typeBuilder.DefineField(node.Name, TupleUtils.MakeTupleType(argTypes), FieldAttributes.Public);
            symbolTable.Register(node.Name, new FieldEntry(field));
        }

        public void Visit(HandlerDeclarationNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(PlanDeclarationNode node)
        {
            var argTypes = node.Args.Select(a => typeof(int)).ToArray();
            methodBuilder = typeBuilder.DefineMethod(node.Name, MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), argTypes);
            il = methodBuilder.GetILGenerator();

            symbolTable.EnterScope();

            var args = node.Args;
            for (int i = 0; i < args.Count; i++)
            {
                symbolTable.Register(args [i].Item1, new ArgumentEntry(i + 1));
            }

            foreach (var n in node.Body)
            {
                n.Accept(this);
            }

            il.Emit(OpCodes.Ret);
			
            symbolTable.ExitScope();

            symbolTable.Register(node.Name, new MethodEntry(methodBuilder));
        }

        public void Visit(PlanInvocationNode node)
        {
            il.Emit(OpCodes.Ldarg_0);
            foreach (var arg in node.Args)
            {
                arg.Accept(this);
            }
            var method = symbolTable.LookupAs<MethodEntry>(node.Name);
            il.Emit(OpCodes.Call, method.Info);
        }

        public void Visit(BeliefQueryNode node)
        {
            var field = symbolTable.LookupAs<FieldEntry>(node.Name);
            var args = node.Args;
            var argTypes = args.Select(a => typeof(int)).ToArray();

            for (int i = 0; i < args.Count; i++)
            {
                var local = il.DeclareLocal(typeof(int));
                symbolTable.Register((args[i] as IdentNode).Name, new LocalEntry(local));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field.Info);
                il.EmitTupleGetItem(argTypes, i);
                il.Emit(OpCodes.Stloc, local);
            }
        }

        public void Visit(BeliefUpdateNode node)
        {
            EmitBeliefUpdate(node.Name, node.Args);
        }

        public void Visit(BinaryOpNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            switch (node.Operator)
            {
                case "+":
                    il.Emit(OpCodes.Add);
                    break;
                case "-":
                    il.Emit(OpCodes.Sub);
                    break;
                case "*":
                    il.Emit(OpCodes.Mul);
                    break;
                case "/":
                    il.Emit(OpCodes.Div);
                    break;
                default:
                    throw new ApplicationException(String.Format("Unknown operator {0}", node.Operator));
            }
        }

        public void Visit(IdentNode node)
        {
            var info = symbolTable.LookupAs(node.Name);
            info.EmitLookup(il);
        }

        public void Visit(NumberNode node)
        {
            il.Emit(OpCodes.Ldc_I4, node.Value);
        }

        #endregion
    }
}
