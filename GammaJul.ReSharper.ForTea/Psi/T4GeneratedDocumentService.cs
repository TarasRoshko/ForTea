#region License
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
using System.Collections.Generic;
using GammaJul.ReSharper.ForTea.Psi.Directives;
using GammaJul.ReSharper.ForTea.Tree;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Impl.Shared;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Web.Generation;
using JetBrains.Util;
#if SDK80
using JetBrains.ReSharper.Psi.Files;
#else
using JetBrains.ReSharper.Psi.Impl.PsiManagerImpl;
#endif

namespace GammaJul.ReSharper.ForTea.Psi {

	/// <summary>
	/// This class will generate a C# code-behind from a T4 file.
	/// </summary>
	[GeneratedDocumentService(typeof(T4ProjectFileType))]
	public partial class T4GeneratedDocumentService : GeneratedDocumentServiceBase {

		private readonly DirectiveInfoManager _directiveInfoManager;
		private readonly FileDependency _fileDependency;
		
		/// <summary>
		/// Generates a C# file from a T4 file.
		/// </summary>
		/// <param name="modificationInfo">The modifications that occurred in the T4 file.</param>
		public override ISecondaryDocumentGenerationResult Generate(PrimaryFileModificationInfo modificationInfo) {
			var t4File = modificationInfo.NewPsiFile as IT4File;
			if (t4File == null)
				return null;
			 
			var generator = new T4CSharpCodeGenerator(t4File, _directiveInfoManager);
			GenerationResult result = generator.Generate();

			LanguageService csharpLanguageService = CSharpLanguage.Instance.LanguageService();
			if (csharpLanguageService == null)
				return null;

			var includedFiles = new OneToSetMap<FileSystemPath, FileSystemPath>();
			includedFiles.AddRange(modificationInfo.SourceFile.GetLocation(), t4File.GetNonEmptyIncludePaths());

			return new T4SecondaryDocumentGenerationResult(
				modificationInfo.SourceFile,
				result.Builder.ToString(),
				csharpLanguageService.LanguageType,
				new RangeTranslatorWithGeneratedRangeMap(result.GeneratedRangeMap),
				csharpLanguageService.GetPrimaryLexerFactory(),
				_fileDependency,
				includedFiles);
		}

		/// <summary>
		/// Gets the secondary PSI language types for a T4 file.
		/// </summary>
		/// <returns>Always <see cref="CSharpLanguage"/>.</returns>
		public override IEnumerable<PsiLanguageType> GetSecondaryPsiLanguageTypes(IProject project) {
			// TODO: handle VB
			return new PsiLanguageType[] { CSharpLanguage.Instance };
		}

		public override bool IsSecondaryPsiLanguageType(IProject project, PsiLanguageType language) {
			return language.Is<CSharpLanguage>();
		}

		/// <summary>
		/// Creates a secondary lexing service for code behind generated files.
		/// </summary>
		/// <param name="solution">The solution.</param>
		/// <param name="mixedLexer">The mixed lexer.</param>
		/// <param name="sourceFile">The source file.</param>
		/// <returns>An instance of <see cref="ISecondaryLexingProcess"/> used to lex the code behind file.</returns>
		public override ISecondaryLexingProcess CreateSecondaryLexingService(ISolution solution, MixedLexer mixedLexer, IPsiSourceFile sourceFile = null) {
			return CSharpLanguage.Instance != null ? new T4SecondaryLexingProcess(CSharpLanguage.Instance, mixedLexer) : null;
		}

		/// <summary>
		/// Gets a lexer factory capable of handling preprocessor directives.
		/// </summary>
		/// <param name="primaryLanguage">The primary language.</param>
		/// <returns>Always <c>null</c> since there is no preprocessor directives in T4 files.</returns>
		public override ILexerFactory LexerFactoryWithPreprocessor(PsiLanguageType primaryLanguage) {
			return null;
		}

		/// <summary>
		/// Reparses the original T4 file.
		/// </summary>
		/// <param name="treeTextRange">The tree text range to reparse.</param>
		/// <param name="newText">The new text to add at <paramref name="treeTextRange"/>.</param>
		/// <param name="rangeTranslator">The range translator.</param>
		/// <returns><c>true</c> if reparse succeeded, <c>false</c> otherwise.</returns>
		protected override bool ReparseOriginalFile(TreeTextRange treeTextRange, string newText, RangeTranslatorWithGeneratedRangeMap rangeTranslator) {
			var t4File = rangeTranslator.OriginalFile as IT4File;
			return t4File != null && t4File.ReParse(treeTextRange, newText) != null;
		}
		
		public T4GeneratedDocumentService([NotNull] FileDependency fileDependency, [NotNull] DirectiveInfoManager directiveInfoManager) {
			_fileDependency = fileDependency;
			_directiveInfoManager = directiveInfoManager;
		}

	}

}