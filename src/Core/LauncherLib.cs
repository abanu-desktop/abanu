using System;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Search;
using System.IO;
using System.Collections.Generic;
using Lucene.Net.Analysis;

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
			storeDir = "/tmp/luc";
			OpenReader();
			CreateCategories();
		}

		internal Dictionary<string, TLauncherCategory> catHash = new Dictionary<string, TLauncherCategory>();

		public void CreateCategories()
		{
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
			AddCategory("applications-system", "applications-system", "System");
			AddCategory("preferences-desktop", "Settings");
			//AddCategory("Favorites");
			//AddCategory("Recently Used");
		}

		public void AddCategory(string icon, string Main, params string[] names)
		{
			var cat = new TLauncherCategory(){ Name = Main, IconName = icon };
			Categories.Add(cat);
			catHash.Add(Main, cat);
			foreach (var nam in names)
				catHash.Add(nam, cat);
		}

		private List<string> dirs = new List<string>();

		public void AddLocation(string loc)
		{
			dirs.Add(loc);
		}

		public void OpenWrite()
		{
			if (!System.IO.Directory.Exists(storeDir))
				System.IO.Directory.CreateDirectory(storeDir);

			var d = FSDirectory.Open(new DirectoryInfo(storeDir));
			writer = new IndexWriter(d, new WhitespaceAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);

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
			System.IO.Directory.Delete(storeDir, true);
		}

		public void Rebuild()
		{
			Clear();
			OpenWrite();
			foreach (var dir in dirs)
				foreach (var f in System.IO.Directory.GetFiles(dir,"*.desktop",SearchOption.AllDirectories))
					AddLink(f);
			FlushWrite();
			CloseWrite();
			OpenReader();
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

		public IEnumerable<TLauncherEntry> ByCategory(TLauncherCategory entry)
		{
			var term = new Term("category", entry.Name);
			var query = new TermQuery(term);
			var hits = reader.Search(query, 1000);

			for (var i = 0; i < hits.TotalHits; i++)
				yield return new TLauncherEntry(reader.Doc(hits.ScoreDocs[i].Doc));
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

		private string Get(string name)
		{
			return doc.Get(name);
		}

		private void Set(string name, string value)
		{
			doc.RemoveField(name);
			doc.Add(new Field(name, value, Field.Store.YES, Field.Index.ANALYZED));
		}

		public string Source { 
			get { 
				return Get("source");
			}
			set { 
				Set("source", value);
			}
		}

		public string Command { 
			get { 
				return Get("command");
			}
			set { 
				Set("command", value);
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

		public byte[] GetIcon { 
			get { 
				return doc.GetBinaryValue("icon");
			}
		}

		public static TLauncherEntry CreateFromFile(string path)
		{
			if (Path.GetExtension(path) == ".lnk")
				return CreateFromFileLnk(path);
			else
				return CreateFromFileDesktop(path);
		}

		private static TLauncherEntry CreateFromFileDesktop(string path)
		{
			var ini = new INIFile(path);
			var entry = new TLauncherEntry();
			entry.Name = ini.GetValue("Desktop Entry", "Name", "");
			entry.Command = ini.GetValue("Desktop Entry", "Exec", "");
			entry.Categories = ini.GetValue("Desktop Entry", "Categories", "");
			entry.UpdateMainCategory();
			return entry;
		}

		private static TLauncherEntry CreateFromFileLnk(string path)
		{
			return null;
		}

	}

}

