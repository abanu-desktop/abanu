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
using Gdk;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace SharpShell.Core
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

		public Dictionary<string, TLauncherCategory> catHash = new Dictionary<string, TLauncherCategory>();

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
				CoreLib.Log(ex.ToString());
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
				CoreLib.Log(ex.ToString());
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
					CoreLib.Log(ex.ToString());
				}
				return null;
			}
		}

		public static TLauncherEntry CreateFromFile(string path)
		{
			CoreLib.Log("ADD LINK: " + path);
			return Factory.Current.ReadLinkFile(path);
		}

	}

}

