//
// TupleUtils.cs
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
using System.Reflection;
using System.Reflection.Emit;

namespace JasonSharp.Backend
{
    public static class TupleUtils
    {
        private static readonly MethodInfo[] tupleCreateMethods = new MethodInfo[8];
        private static readonly Dictionary<Type[], MethodInfo> tupleGetItemMethods = new Dictionary<Type[], MethodInfo>();

        static TupleUtils()
        {
            foreach (var meth in typeof(Tuple).GetMethods())
            {
                if (meth.Name == "Create")
                {
                    tupleCreateMethods[meth.GetGenericArguments().Length - 1] = meth;
                }
            }
        }

        public static Type MakeTupleType(Type[] argTypes)
        {
            var tupleOf = Type.GetType("System.Tuple`" + argTypes.Length);
            var genericTypeOf = tupleOf.MakeGenericType(argTypes);
            return genericTypeOf;
        }

        public static void EmitTupleCreate(this ILGenerator il, Type[] argTypes)
        {
            if (argTypes.Length >= tupleCreateMethods.Length)
            {
                throw new ApplicationException(String.Format("Can't have more than {0} arguments. Yeah, it sucks, I know.", tupleCreateMethods.Length));
            }

            var meth = tupleCreateMethods[argTypes.Length - 1];
            var createMeth = meth.MakeGenericMethod(argTypes);

            il.Emit(OpCodes.Call, createMeth);
        }

        public static void EmitTupleGetItem(this ILGenerator il, Type[] argTypes, int idx)
        {
            MethodInfo getItemMeth;

            if (!tupleGetItemMethods.TryGetValue(argTypes, out getItemMeth))
            {
                var tupleOf = TupleUtils.MakeTupleType(argTypes);
                getItemMeth = tupleOf.GetMethod("get_Item" + (idx + 1));
                tupleGetItemMethods.Add(argTypes, getItemMeth);
            }

            il.Emit(OpCodes.Call, getItemMeth);
        }
    }
}

