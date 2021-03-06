﻿//
// CodeGenerator.cs
//
// Author:
//       Alex Muscar <muscar@gmail.com>
//
// Copyright (c) 2013 Alex Muscar
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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

        public abstract void EmitStore(ILGenerator il);
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

        public override void EmitStore(ILGenerator il)
        {
            il.Emit(OpCodes.Starg, Info);
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

        public override void EmitStore(ILGenerator il)
        {
            il.Emit(OpCodes.Stloc, Info);
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

        public override void EmitStore(ILGenerator il)
        {
            il.Emit(OpCodes.Stfld, Info);
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

        public override void EmitStore(ILGenerator il)
        {
            throw new InvalidOperationException();
        }
    }

    public class SemanticErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public SemanticErrorEventArgs(string message)
        {
            Message = message;
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

        public event EventHandler<SemanticErrorEventArgs> CodegenError;

        public bool HasErrors { get; private set; }

        public string ModuleName { get { return moduleBuilder.Name; } }

        public CodeGenerator(string moduleName)
        {
            var assemblyName = new AssemblyName { Name = moduleName };
            assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName + ".dll");
        }

        protected virtual void OnCodeGenError(SemanticErrorEventArgs e)
        {
            if (CodegenError != null)
            {
                CodegenError(this, e);
            }
            HasErrors = true;
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

        void EmitBeliefUpdate(string name, IList<INode> args)
        {
            SymbolTableEntry field;
            if (symbolTable.TryLookup(name, out field))
            {
                var argTypes = new Type[args.Count];

                il.Emit(OpCodes.Ldarg_0);

                for (int i = 0; i < args.Count; i++)
                {
                    args[i].Accept(this);
                    argTypes[i] = typeof(int);
                }

                il.EmitTupleCreate(argTypes);
                field.EmitStore(il);
            }
            else
            {
                OnCodeGenError(new SemanticErrorEventArgs(String.Format("`{0}` is not in scope", name)));
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
            OnCodeGenError(new SemanticErrorEventArgs("Code generation for handlers is not implemented"));
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
                symbolTable.Register(args[i].Item1, new ArgumentEntry(i + 1));
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
            SymbolTableEntry method;
            if (symbolTable.TryLookup(node.Name, out method))
            {
                il.Emit(OpCodes.Call, (method as MethodEntry).Info);
            }
            else
            {
                OnCodeGenError(new SemanticErrorEventArgs(String.Format("`{0}` is not in scope", node.Name)));
            }
        }

        public void Visit(BeliefQueryNode node)
        {
            SymbolTableEntry field;

            if (symbolTable.TryLookup(node.Name, out field))
            {
                var args = node.Args;
                var argTypes = args.Select(a => typeof(int)).ToArray();

                for (int i = 0; i < args.Count; i++)
                {
                    var local = il.DeclareLocal(typeof(int));
                    symbolTable.Register((args[i] as IdentNode).Name, new LocalEntry(local));
                    field.EmitLookup(il);
                    il.EmitTupleGetItem(argTypes, i);
                    il.Emit(OpCodes.Stloc, local);
                }
            }
            else
            {
                OnCodeGenError(new SemanticErrorEventArgs(String.Format("`{0}` is not in scope", node.Name)));
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
                    OnCodeGenError(new SemanticErrorEventArgs(String.Format("Unknown operator {0}", node.Operator)));
                    break;
            }
        }

        public void Visit(IdentNode node)
        {
            SymbolTableEntry info;
            if (symbolTable.TryLookup(node.Name, out info))
            {
                info.EmitLookup(il);
            }
            else
            {
                OnCodeGenError(new SemanticErrorEventArgs(String.Format("`{0}` is not in scope", node.Name)));
            }
        }

        public void Visit(NumberNode node)
        {
            il.Emit(OpCodes.Ldc_I4, node.Value);
        }
        #endregion
    }
}
