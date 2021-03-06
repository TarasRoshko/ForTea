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
using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.PluginSupport;
using JetBrains.DataFlow;
using JetBrains.Extension;
using JetBrains.UI.Updates;
using JetBrains.VSIntegration.Updates;

namespace GammaJul.ReSharper.ForTea.Updates {

	[ShellComponent]
	public class UpdatesNotifier {

		private readonly UpdatesCategory _category;

		private static void CustomizeLocalEnvironmentInfo([NotNull] OutEventArgs<object> args) {
			var reSharperInfo = args.Out as UpdateLocalEnvironmentInfoVs;
			if (reSharperInfo == null)
				args.Out = null;
			else {
				var rootInfo = new RootUpdateInfo { ReSharper = reSharperInfo };
				FillPlugInInfo(rootInfo.Plugin);
				args.Out = rootInfo;
			}
		}

		private static void FillPlugInInfo([NotNull] UpdateLocalEnvironmentInfo.ProductSubInfo info) {
			Assembly assembly = typeof(UpdatesNotifier).Assembly;
			Version version = assembly.GetName().Version;
			info.CompanyName = assembly.GetCustomAttribute<PluginVendorAttribute>(false).Text;
			info.Name = assembly.GetCustomAttribute<PluginTitleAttribute>(false).Text;
			info.Version = new UpdateLocalEnvironmentInfo.VersionSubInfo(version);
			info.FullName = info.Name + " " + version.ToString(3);
		}

		/// <summary>
		/// From resharper-nuget plugin:
		/// ReSharper downloads and evaluates the xslt on a regular basis (every 24 hours),
		/// but doesn't re-evaluate it after an install (it doesn't know when something is
		/// installed!) so if there's a reminder to download this or an older version, remove it.
		/// </summary>
		private void RemoveStaleUpdateNotification() {
			Version thisVersion = typeof(UpdatesNotifier).Assembly.GetName().Version;
			var updateInfo = _category.UpdateInfos.FirstOrDefault(c => new Version(c.Data.ProductVersion) <= thisVersion);
			if (updateInfo != null)
				_category.UpdateInfos.Remove(updateInfo);
		}

		public UpdatesNotifier([NotNull] Lifetime lifetime, [NotNull] UpdatesManager updatesManager) {
			_category = updatesManager.Categories.AddOrActivate("ForTea", new Uri("https://raw.github.com/MrJul/ForTea/master/Updates.xslt"));
			_category.CustomizeLocalEnvironmentInfo.Advise(lifetime, CustomizeLocalEnvironmentInfo);
			RemoveStaleUpdateNotification();
		}

	}

}