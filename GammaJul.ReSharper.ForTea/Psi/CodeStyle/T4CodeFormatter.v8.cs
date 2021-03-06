﻿#region License

//    Copyright 2012 Julien Lebosquain
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion


using GammaJul.ReSharper.ForTea.Parsing;
using JetBrains.ReSharper.Psi.Tree;

namespace GammaJul.ReSharper.ForTea.Psi.CodeStyle {

	public partial class T4CodeFormatter {

		public override ITreeNode CreateSpace(string indent, ITreeNode replacedSpace) {
			return T4TokenNodeTypes.Space.Create(indent);
		}

		public override ITreeNode CreateNewLine() {
			T4TokenNodeType nodeType = T4TokenNodeTypes.NewLine;
			return nodeType.Create(nodeType.TokenRepresentation);
		}

	}

}