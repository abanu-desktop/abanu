using System;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Analysis.Standard;
using System.IO;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Gtk;
using Gdk;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace PanelShell
{

	public class TLauncherIndex
	{
		private IndexWriter writer;
		private IndexSearcher reader;

		private string storeDir;

		public static TLauncherIndex Current;

		public TLauncherIndex()
		{
			Current = this;
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				storeDir = "/tmp/luc";
			else
				storeDir = @"C:\temp\luc";

			CreateCategories();
		}

		internal Dictionary<string, TLauncherCategory> catHash = new Dictionary<string, TLauncherCategory>();

		public void CreateCategories()
		{
			AddCategory("", "All").meta = true;
			AddCategory("", "Recently Used").meta = true;
			AddCategory("", "Favorites").meta = true;
			AddCategory("applications-office", "Multimedia", "AudioVideo");
			AddCategory("applications-accessories", "Accessories", "Utility");
			AddCategory("applications-graphics", "Graphics");
			AddCategory("applications-other", "Other");
			AddCategory("applications-games", "Game");
			AddCategory("applications-internet", "Internet", "Network");
			AddCategory("applications-office", "Office");
			AddCategory("applications-development", "Development");
			AddCategory("applications-engineering", "Engineering");
			AddCategory("applications-science", "Education");
			AddCategory("applications-system", "System");
			AddCategory("preferences-desktop", "Settings");
			//AddCategory("Favorites");
			//AddCategory("Recently Used");
		}

		public TLauncherCategory AddCategory(string icon, string Main, params string[] names)
		{
			var cat = new TLauncherCategory(){ Name = Main, IconName = icon };
			Categories.Add(cat);
			catHash.Add(Main, cat);
			foreach (var nam in names)
				catHash.Add(nam, cat);
			return cat;
		}

		private List<string> dirs = new List<string>();

		public void AddLocation(string loc)
		{
			dirs.Add(Environment.ExpandEnvironmentVariables(loc));
		}

		public void AddLocations()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				AddLocation("/usr/share/applications");
			} else {
				AddLocation(@"%programdata%\Microsoft\Windows\Start Menu");
				AddLocation(@"%appdata%\Microsoft\Windows\Start Menu");
			}
		}

		public void OpenWrite()
		{
			if (!System.IO.Directory.Exists(storeDir))
				System.IO.Directory.CreateDirectory(storeDir);

			var d = FSDirectory.Open(new DirectoryInfo(storeDir));
			writer = new IndexWriter(d, new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT), true, IndexWriter.MaxFieldLength.UNLIMITED);

			//reader = new IndexSearcher(d);
		}

		public void CloseWrite()
		{
			if (writer != null) {
				writer.Dispose();
				writer = null;
			}
		}

		public void Clear()
		{
			CloseWrite();
			CloseReader();
			if (System.IO.Directory.Exists(storeDir))
				System.IO.Directory.Delete(storeDir, true);
		}

		public void Rebuild()
		{
			Clear();
			OpenWrite();
			foreach (var dir in dirs)
				foreach (var f in GetFiles(dir,new string[]{"*.desktop", "*.lnk"}))
					AddLink(f);
			FlushWrite();
			CloseWrite();
			OpenReader();
		}

		private IEnumerable<string> GetFiles(string dir, string[] pattern)
		{
			var filesList = new List<string[]>();
			try {
				foreach (var pat in pattern) {
					string[] files = new string[]{ };
					files = System.IO.Directory.GetFiles(dir, pat);
					filesList.Add(files);
				}
			} catch (UnauthorizedAccessException ex) {
			}

			foreach (var files in filesList)
				foreach (var f in files)
					yield return f;

			string[] subDirs = new string[]{ };

			try {
				subDirs = System.IO.Directory.GetDirectories(dir);
			} catch (UnauthorizedAccessException ex) {
			}

			foreach (var subDir in subDirs) {
				foreach (var f in GetFiles(subDir, pattern))
					yield return f;
			}
		}

		public void FlushWrite()
		{
			if (writer != null) {
				writer.Optimize();
				writer.Flush(true, true, true);
			}

			//var d = FSDirectory.Open(new DirectoryInfo(storeDir));
			//reader = new IndexSearcher(d);
		}

		private void OpenReader()
		{
			CloseReader();

			var d = FSDirectory.Open(new DirectoryInfo(storeDir));
			reader = new IndexSearcher(d);
		}

		private void CloseReader()
		{
			if (reader != null) {
				reader.Close();
				reader = null;
			}
		}

		public void AddLink(string file)
		{
			try {
				var entry = TLauncherEntry.CreateFromFile(file);
				writer.AddDocument(entry.doc);
			} catch (Exception ex) {
				AppLib.log(ex.ToString());
			}
		}

		public IEnumerable<TLauncherEntry> All()
		{

			for (var i = 0; i < reader.MaxDoc; i++)
				yield return new TLauncherEntry(reader.Doc(i));
		}

		public IEnumerable<TLauncherEntry> BySearch(string txt)
		{
			if (string.IsNullOrEmpty(txt))
				yield break;

			var exp = string.Format("label:*{0}* description:*{0}* command-file:*{0}*", txt);
			foreach (var itm in BySearchExpression(exp))
				yield return itm;
		}

		public IEnumerable<TLauncherEntry> BySearchExpression(string exp)
		{
			if (string.IsNullOrEmpty(exp))
				yield break;

			TopDocs hits = null;
			try {
				
				var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_CURRENT, new string[] {
					"label",
					"description",
					"category"
				}, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT));
				parser.AllowLeadingWildcard = true;
				hits = reader.Search(parser.Parse(exp), 1000);
			} catch (Exception ex) {
				AppLib.log(ex.ToString());
			}
			if (hits != null)
				for (var i = 0; i < hits.TotalHits; i++)
					yield return new TLauncherEntry(reader.Doc(hits.ScoreDocs[i].Doc));
		}

		public IEnumerable<TLauncherEntry> ByCategory(TLauncherCategory entry)
		{
			if (entry.Name == "All") {
				return All();
			}

			return BySearchExpression("category:" + entry.Name);
		}

		public List<TLauncherCategory> Categories = new List<TLauncherCategory>();

	}

	public class TLauncherCategory
	{
		public string Name;
		public string IconName;

		public bool HasIcon { 
			get {
				return !string.IsNullOrEmpty(IconName);
			}
		}

		public bool meta = false;

	}

	public class TLauncherEntry
	{

		public TLauncherEntry(Document doc)
		{
			this.doc = doc;
		}

		public TLauncherEntry()
		{
			this.doc = new Document();
		}

		internal Document doc;

		public string Name { 
			get { 
				return Get("label");
			}
			set { 
				Set("label", value);
			}
		}

		public string Description { 
			get { 
				return Get("description");
			}
			set { 
				Set("description", value);
			}
		}

		private string Get(string name)
		{
			return doc.Get(name);
		}

		private void Set(string name, string value)
		{
			doc.RemoveField(name);
			doc.Add(new Field(name, value, Field.Store.YES, Field.Index.ANALYZED));
		}

		private void SetBinary(string name, byte[] value)
		{
			doc.RemoveField(name);
			doc.Add(new Field(name, value, Field.Store.YES));
		}

		public string Source { 
			get { 
				return Get("source");
			}
			set { 
				Set("source", value);
			}
		}

		public string CommandFile { 
			get { 
				return Get("command-file");
			}
			set { 
				Set("command-file", value);
			}
		}

		public string CommandPath { 
			get { 
				return Get("command-path");
			}
			set { 
				Set("command-path", value);
			}
		}

		public string CommandArgs { 
			get { 
				return Get("command-args");
			}
			set { 
				Set("command-args", value);
			}
		}

		public string Categories { 
			get { 
				return Get("categories");
			}
			set { 
				Set("categories", value);
			}
		}

		public string[] CategoriesArray { 
			get {
				return Categories.Split(new char[]{ ';' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		public string MainCategory {
			get { 
				return Get("category");
			}
			set {
				Set("category", value);
			}
		}

		public void UpdateMainCategory()
		{
			foreach (var cat in CategoriesArray) {
				if (TLauncherIndex.Current.catHash.ContainsKey(cat)) {
					MainCategory = TLauncherIndex.Current.catHash[cat].Name;
					return;
				}
			}
			MainCategory = "Other";
		}

		public byte[] IconStored { 
			get {
				return doc.GetBinaryValue("icon-data");
			}
			set {
				SetBinary("icon-data", value);	
			}
		}

		public string IconName { 
			get {
				return Get("icon-name");
			}
			set { 
				Set("icon-name", value);
			}
		}

		public Pixbuf GetIconPixBuf()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				var file = "/usr/share/icons/gnome/32x32/apps/" + IconName + ".png";
				if (File.Exists(file)) {
					return new Pixbuf(file);
				} else
					return null;
			} else {
				try {
					var data = IconStored;
					if (data == null || data.Length == 0)
						return null;
					else
						return new Pixbuf(data);
				} catch (Exception ex) {
					AppLib.log(ex.ToString());
				}
				return null;
			}
		}

		public static TLauncherEntry CreateFromFile(string path)
		{
			AppLib.log("ADD LINK: " + path);
			if (Path.GetExtension(path).ToLower() == ".lnk")
				return CreateFromFileLnk(path);
			else
				return CreateFromFileDesktop(path);
			
		}

		private static TLauncherEntry CreateFromFileDesktop(string path)
		{
			var ini = new INIFile(path);
			var entry = new TLauncherEntry();
			entry.Name = ini.GetValue("Desktop Entry", "Name", "");
			var cmd = ini.GetValue("Desktop Entry", "Exec", "");
			entry.CommandPath = Path.GetDirectoryName(cmd);
			entry.CommandFile = Path.GetFileName(cmd);
			entry.CommandArgs = ""; //TODO
			entry.Categories = ini.GetValue("Desktop Entry", "Categories", "");
			entry.IconName = ini.GetValue("Desktop Entry", "Icon", "");
			entry.Description = ini.GetValue("Desktop Entry", "Comment", "");
			entry.UpdateMainCategory();
			return entry;
		}

		private static TLauncherEntry CreateFromFileLnk(string path)
		{
			var entry = new TLauncherEntry();

			string name, command, args, description, iconLocation;
			int iconIndex;

			ResolveShortcut(path, out name, out command, out args, out description, out iconLocation, out iconIndex);

			if (!File.Exists(command)) {
				var newCommand = command.Replace("\\Program Files (x86)", "\\Program Files").Replace("\\system32", "Sysnative");
				if (File.Exists(newCommand)) {
					command = newCommand;
				} else {
					AppLib.log("COMMAND NOT FOUND: '" + command + "'");
				}
			}

			if (string.IsNullOrEmpty(iconLocation)) {
				iconLocation = command;
				iconIndex = 0;
			}

			//if (name == "Steam") {
			//	AppLib.log("########################################################" + iconLocation);
			//}

			iconLocation = Environment.ExpandEnvironmentVariables(iconLocation);

			if (!File.Exists(iconLocation)) {
				//AppLib.log("COMMAND NOT FOUND: " + command);
				iconLocation = iconLocation.Replace("\\Program Files (x86)", "\\Program Files");
			}

			if (File.Exists(iconLocation)) {


				var ext = new TsudaKageyu.IconExtractor(iconLocation);
				var ico = ext.GetIcon(iconIndex);
				AppLib.log(ico.Size.ToString());
				var ms = new MemoryStream();
				ico.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				entry.IconStored = ms.ToArray();
			} else {
				AppLib.log("ICON LOCATION NOT FOUND: " + iconLocation);
			}

			entry.Name = name;
			entry.CommandPath = Path.GetDirectoryName(command);
			entry.CommandFile = Path.GetFileName(command);
			entry.CommandArgs = args;
			entry.Description = description;

			return entry;
		}

		#region win32

		[DllImport("shfolder.dll", CharSet = CharSet.Auto)]
		internal static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

		[Flags()]
		enum SLGP_FLAGS
		{
			/// <summary>Retrieves the standard short (8.3 format) file name</summary>
			SLGP_SHORTPATH = 0x1,
			/// <summary>Retrieves the Universal Naming Convention (UNC) path name of the file</summary>
			SLGP_UNCPRIORITY = 0x2,
			/// <summary>Retrieves the raw path name. A raw path is something that might not exist and may include environment variables that need to be expanded</summary>
			SLGP_RAWPATH = 0x4
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct WIN32_FIND_DATAW
		{
			public uint dwFileAttributes;
			public long ftCreationTime;
			public long ftLastAccessTime;
			public long ftLastWriteTime;
			public uint nFileSizeHigh;
			public uint nFileSizeLow;
			public uint dwReserved0;
			public uint dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public string cAlternateFileName;
		}

		[Flags()]
		enum SLR_FLAGS
		{
			/// <summary>
			/// Do not display a dialog box if the link cannot be resolved. When SLR_NO_UI is set,
			/// the high-order word of fFlags can be set to a time-out value that specifies the
			/// maximum amount of time to be spent resolving the link. The function returns if the
			/// link cannot be resolved within the time-out duration. If the high-order word is set
			/// to zero, the time-out duration will be set to the default value of 3,000 milliseconds
			/// (3 seconds). To specify a value, set the high word of fFlags to the desired time-out
			/// duration, in milliseconds.
			/// </summary>
			SLR_NO_UI = 0x1,
			/// <summary>Obsolete and no longer used</summary>
			SLR_ANY_MATCH = 0x2,
			/// <summary>If the link object has changed, update its path and list of identifiers.
			/// If SLR_UPDATE is set, you do not need to call IPersistFile::IsDirty to determine
			/// whether or not the link object has changed.</summary>
			SLR_UPDATE = 0x4,
			/// <summary>Do not update the link information</summary>
			SLR_NOUPDATE = 0x8,
			/// <summary>Do not execute the search heuristics</summary>
			SLR_NOSEARCH = 0x10,
			/// <summary>Do not use distributed link tracking</summary>
			SLR_NOTRACK = 0x20,
			/// <summary>Disable distributed link tracking. By default, distributed link tracking tracks
			/// removable media across multiple devices based on the volume name. It also uses the
			/// Universal Naming Convention (UNC) path to track remote file systems whose drive letter
			/// has changed. Setting SLR_NOLINKINFO disables both types of tracking.</summary>
			SLR_NOLINKINFO = 0x40,
			/// <summary>Call the Microsoft Windows Installer</summary>
			SLR_INVOKE_MSI = 0x80
		}


		/// <summary>The IShellLink interface allows Shell links to be created, modified, and resolved</summary>
		[ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
		interface IShellLinkW
		{
			/// <summary>Retrieves the path and file name of a Shell link object</summary>
			void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out WIN32_FIND_DATAW pfd, SLGP_FLAGS fFlags);

			/// <summary>Retrieves the list of item identifiers for a Shell link object</summary>
			void GetIDList(out IntPtr ppidl);

			/// <summary>Sets the pointer to an item identifier list (PIDL) for a Shell link object.</summary>
			void SetIDList(IntPtr pidl);

			/// <summary>Retrieves the description string for a Shell link object</summary>
			void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);

			/// <summary>Sets the description for a Shell link object. The description can be any application-defined string</summary>
			void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

			/// <summary>Retrieves the name of the working directory for a Shell link object</summary>
			void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

			/// <summary>Sets the name of the working directory for a Shell link object</summary>
			void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

			/// <summary>Retrieves the command-line arguments associated with a Shell link object</summary>
			void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

			/// <summary>Sets the command-line arguments for a Shell link object</summary>
			void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

			/// <summary>Retrieves the hot key for a Shell link object</summary>
			void GetHotkey(out short pwHotkey);

			/// <summary>Sets a hot key for a Shell link object</summary>
			void SetHotkey(short wHotkey);

			/// <summary>Retrieves the show command for a Shell link object</summary>
			void GetShowCmd(out int piShowCmd);

			/// <summary>Sets the show command for a Shell link object. The show command sets the initial show state of the window.</summary>
			void SetShowCmd(int iShowCmd);

			/// <summary>Retrieves the location (path and index) of the icon for a Shell link object</summary>
			int GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
			                    int cchIconPath, out int piIcon);

			/// <summary>Sets the location (path and index) of the icon for a Shell link object</summary>
			void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

			/// <summary>Sets the relative path to the Shell link object</summary>
			void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);

			/// <summary>Attempts to find the target of a Shell link, even if it has been moved or renamed</summary>
			void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

			/// <summary>Sets the path and file name of a Shell link object</summary>
			void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);

		}

		[ComImport, Guid("0000010c-0000-0000-c000-000000000046"),
			InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IPersist
		{
			[PreserveSig]
			void GetClassID(out Guid pClassID);
		}


		[ComImport, Guid("0000010b-0000-0000-C000-000000000046"),
			InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IPersistFile : IPersist
		{
			new void GetClassID(out Guid pClassID);

			[PreserveSig]
			int IsDirty();

			[PreserveSig]
			void Load([In, MarshalAs(UnmanagedType.LPWStr)]
				string pszFileName, uint dwMode);

			[PreserveSig]
			void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
			          [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);

			[PreserveSig]
			void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

			[PreserveSig]
			void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
		}

		const uint STGM_READ = 0;
		const int MAX_PATH = 260;

		// CLSID_ShellLink from ShlGuid.h
		[
			ComImport(),
			Guid("00021401-0000-0000-C000-000000000046")
		]
		public class ShellLink
		{
		}

		public static void ResolveShortcut(string filename, out string name, out string command, out string args, out string description, out string iconLocation, out int iconIndex)
		{
			ShellLink link = new ShellLink();
			((IPersistFile)link).Load(filename, STGM_READ);
			// TODO: if I can get hold of the hwnd call resolve first. This handles moved and renamed files.  
			// ((IShellLinkW)link).Resolve(hwnd, 0) 

			StringBuilder sb = new StringBuilder(MAX_PATH);
			WIN32_FIND_DATAW data = new WIN32_FIND_DATAW();
			((IShellLinkW)link).GetPath(sb, sb.Capacity, out data, 0);
			command = sb.ToString(); 

			sb = new StringBuilder(MAX_PATH); //MAX_PATH?
			((IShellLinkW)link).GetArguments(sb, sb.Capacity);
			args = sb.ToString(); 

			description = "";
			try {
				sb = new StringBuilder(MAX_PATH); //MAX_PATH?
				((IShellLinkW)link).GetDescription(sb, sb.Capacity);
				description = sb.ToString(); 
			} catch (COMException ex) {
			}

			sb = new StringBuilder(MAX_PATH); //MAX_PATH?
			var hresult = ((IShellLinkW)link).GetIconLocation(sb, sb.Capacity, out iconIndex);
			iconLocation = sb.ToString(); 
			if (hresult != 0) {
				AppLib.log("GetIconLocation result: " + hresult);
			}

			name = Testing.TestGetLocalizedName.GetName(filename);
		}

		#endregion
	}

}

