using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis; // for Analyser
using Lucene.Net.Documents; // for Document and Field
using Lucene.Net.Index; //for Index Writer
using Lucene.Net.Store; //for Directory
using Lucene.Net.Search; // for IndexSearcher
using Lucene.Net.QueryParsers;  // for QueryParser
using Newtonsoft.Json;
using System.IO;

namespace LuceneApplication {
    class LuceneApplication {
        Lucene.Net.Store.Directory luceneIndexDirectory;
        Lucene.Net.Analysis.Analyzer analyzer;
        Lucene.Net.Index.IndexWriter writer;
        Lucene.Net.Search.IndexSearcher searcher;
        Lucene.Net.QueryParsers.QueryParser parser;

        const Lucene.Net.Util.Version VERSION = Lucene.Net.Util.Version.LUCENE_30;
        const string TEXT_FN = "passage_text";

        public LuceneApplication() {
            luceneIndexDirectory = null;
            analyzer = null;
            writer = null;
        }

        /// <summary>
        /// Creates the index at indexPath
        /// </summary>
        /// <param name="indexPath">Directory path to create the index</param>
        public void CreateIndex(string indexPath) {
            luceneIndexDirectory = Lucene.Net.Store.FSDirectory.Open(indexPath);
            analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(VERSION);
            IndexWriter.MaxFieldLength mfl = new IndexWriter.MaxFieldLength(IndexWriter.DEFAULT_MAX_FIELD_LENGTH);
            //IndexDeletionPolicy p;

            writer = new Lucene.Net.Index.IndexWriter(luceneIndexDirectory, analyzer, true, mfl);

        }

        /// <summary>
        /// Indexes the given text
        /// </summary>
        /// <param name="text">Text to index</param>
        public void IndexText(PassageCollection passage) {
            Lucene.Net.Documents.Document doc = new Document();
            foreach (Passage p in passage.passages)
            {
                Lucene.Net.Documents.Field is_selected = new Field("is_selected", p.is_selected.ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Lucene.Net.Documents.Field url = new Field("url", p.url, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Lucene.Net.Documents.Field passage_text = new Field("passage_text", p.passage_text, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Lucene.Net.Documents.Field passage_ID = new Field("passage_ID", p.passage_ID.ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                doc.Add(is_selected);
                doc.Add(url);
                doc.Add(passage_text);
                doc.Add(passage_ID);
                writer.AddDocument(doc);
                doc.RemoveField("is_selected");
                doc.RemoveField("url");
                doc.RemoveField("passage_text");
                doc.RemoveField("passage_ID");
            }

            Lucene.Net.Documents.Field query_id = new Field("query_id", passage.query_id.ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            Lucene.Net.Documents.Field query_type = new Field("query_type", passage.query_type, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            Lucene.Net.Documents.Field query = new Field("query", passage.query.ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            foreach (string s in passage.answers)
            {
                Lucene.Net.Documents.Field answers = new Field("answers", s, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                doc.Add(answers);
            }

            doc.Add(query_id);
            doc.Add(query_type);
            doc.Add(query);
            writer.AddDocument(doc);
        }

        /// <summary>
        /// Flushes buffer and closes the index
        /// </summary>
        public void CleanUpIndexer() {
            writer.Optimize();
            writer.Flush(true, true, true);
            writer.Dispose();
        }


        /// <summary>
        /// Initialises the searcher object
        /// </summary>
        public void CreateSearcher() {
            searcher = new IndexSearcher(luceneIndexDirectory);
        }

        /// <summary>
        /// Initialises the parser object
        /// </summary>
        public void CreateParser() {
            string[] multiFields = new string[] { "url", "passage_text", "query_id", "query", "answers" };
           // parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "passage_ID", analyzer);
            parser = new MultiFieldQueryParser(VERSION, multiFields, analyzer);
            
        }

        /// <summary>
        /// Closes the index after searching
        /// </summary>
        public void CleanUpSearch() {
            searcher.Dispose();
        }


        // Activity 6
        /// <summary>
        /// Searches the index with the specified query text
        /// </summary>
        /// <param name="querytext">Text to search the index</param>
        /// <returns></returns>
        public TopDocs SearchIndex(string querytext) {

            System.Console.WriteLine("Searching for " + querytext);
            querytext = querytext.ToLower();
            Query query = parser.Parse(querytext);
            TopDocs results = searcher.Search(query, 100);
            System.Console.WriteLine("Number of results is " + results.TotalHits);
            return results;

        }


        // Activity 7
        /// <summary>
        /// Outputs results to the screen
        /// </summary>
        /// <param name="results">Search results</param>
        public void DisplayResults(TopDocs results) {
            int rank = 0;
            foreach (ScoreDoc scoreDoc in results.ScoreDocs) {
                rank++;
                
                // retrieve the document from the 'ScoreDoc' object
                Lucene.Net.Documents.Document doc = searcher.Doc(scoreDoc.Doc);
                if (doc.GetField("passage_text") != null)
                {
                    string Result_passage_text = doc.Get("passage_text").ToString();
                    string Result_url = doc.Get("url").ToString();
                    Console.WriteLine("\n\nRank:    " + rank + "\nPassage text:   " + Result_passage_text);
                    Console.WriteLine("url:     " + Result_url);
                }
                else
                {
                    string Result_query = doc.Get("query").ToString();
                    string Result_answers = doc.Get("answers").ToString();
                    Console.WriteLine("\n\nRank:    " + rank + "\nQuery:   " + Result_query);
                    Console.WriteLine("answers:     " + Result_answers);
                }
                //Console.WriteLine("Rank " + rank + " score " + scoreDoc.Score + " text " + myFieldValue);
                

            }
        }

        public List<PassageCollection> JsonFileRead()
        {
            string filePath = @"C:\Users\s5128503\Downloads\collection.json";

            StreamReader streamReader = new StreamReader(filePath);
            JsonTextReader JTextreader = new JsonTextReader(streamReader);
            PassageCollection p;
            
            List<PassageCollection> PassageList = new List<PassageCollection>();
            JTextreader.SupportMultipleContent = true;

            var serializer = new JsonSerializer();
            while (JTextreader.Read())
            {
                if (JTextreader.TokenType == JsonToken.StartObject)
                {
                    p = serializer.Deserialize<PassageCollection>(JTextreader);
                    PassageList.Add(p);
                }
            }
            return PassageList;
        }
        static void Main(string[] args) {

            System.Console.WriteLine("Hello Lucene.Net");

            LuceneApplication myLuceneApp = new LuceneApplication();
            List<PassageCollection> PassageList = new List<PassageCollection>();

            string indexPath = @"c:\temp\Week5Index";

            myLuceneApp.CreateIndex(indexPath);
            PassageList = myLuceneApp.JsonFileRead();
            System.Console.WriteLine("Adding Documents to Index");

            foreach (PassageCollection s in PassageList) {

                System.Console.WriteLine("Adding passage to Index");
                myLuceneApp.IndexText(s);
            }

            System.Console.WriteLine("All documents added.\n");

            // clean up
            myLuceneApp.CleanUpIndexer();


            myLuceneApp.CreateSearcher();
            myLuceneApp.CreateParser();

            // Activity 6
            myLuceneApp.DisplayResults(myLuceneApp.SearchIndex("\nhow long you need for ")) ;

         
            myLuceneApp.CleanUpSearch();
            System.Console.ReadLine();


        }
    }
}