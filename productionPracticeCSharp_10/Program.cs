using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace productionPracticeCSharp_10
{
	class Program
	{
		static void Main(string[] args)
		{
            try
            {
                List<Edition> editions = new List<Edition>();
                List<Book> books = new List<Book>();
                List<Magazine> magazines = new List<Magazine>();
                #region ReadFromFile
                string[] lines = File.ReadAllLines("../../file.txt");
                for (int i = 0; i < lines.Length; i = i + 2)
                {
                    EditionType editionType;
                    try
                    {
                        if (!Enum.TryParse(lines[i], out editionType))
                        {
                            throw new Exception("Wrong type of edition!");
                        }
                        else if (editionType == EditionType.Book)
                        {
                            Book book = new Book();
                            book.ParseLine(lines[i + 1]);
                            books.Add(book);
                            editions.Add(book);
                        }
                        else if (editionType == EditionType.Magazine)
                        {
                            Magazine magazine = new Magazine();
                            magazine.ParseLine(lines[i + 1]);
                            magazines.Add(magazine);
                            editions.Add(magazine);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex.Message);
                    }
                }
                #endregion
                OnAction("Data in file:");
                outputList(editions);
                Console.WriteLine();

                #region Menu
                string key;
            loop:
                OnAction("Menu");
                Console.Write("Input key(sort, binary, xml): ");
                key = Console.ReadLine().ToLower();
                switch (key)
                {
                    case "sort":
                        {
                            #region Sort
                            editions = editions.OrderBy(e => e.Name).ToList();
                            OnAction("Data in file(sorted by Name):");
                            outputList(editions);
                            #endregion
                            goto loop;
                        }
                    case "binary":
                        {
                            #region Binary
                            BinaryFormatter formatter = new BinaryFormatter();
                            using (FileStream fs = new FileStream("editions.dat", FileMode.OpenOrCreate))
                            {
                                formatter.Serialize(fs, editions);
                                OnAction("Object was serialized BINARY!");
                            }
                            List<Edition> deserializedEditions = new List<Edition>();
                            using (FileStream fs = new FileStream("editions.dat", FileMode.OpenOrCreate))
                            {
                                deserializedEditions = (List<Edition>)formatter.Deserialize(fs);
                                OnAction("Object was deserialized BINARY!");
                                outputList(deserializedEditions);

                            }
                            #endregion
                            goto loop;
                        }
                    case "xml":
                        {
                            #region Xml
                            XmlSerializer xmlSerializer = new XmlSerializer(editions.GetType(), new Type[] { typeof(Book), typeof(Magazine) });
                            using (FileStream fs = new FileStream("editions.xml", FileMode.OpenOrCreate))
                            {
                                xmlSerializer.Serialize(fs, editions);
                                OnAction("Object was serialized XML!");
                            }
                            using (FileStream fs = new FileStream("editions.xml", FileMode.OpenOrCreate))
                            {
                                List<Edition> deserializedEditions;
                                deserializedEditions = (List<Edition>)xmlSerializer.Deserialize(fs);
                                OnAction("Object was deserialized XML!");
                                outputList(deserializedEditions);
                            }
                            #endregion
                            goto loop;
                        }
                    default:
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }

			Console.ReadLine();
		}

		public static void OnError(string text)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(text);
			Console.ResetColor();
		}

		public static void OnAction(string text)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(text);
			Console.ResetColor();
		}

		public static void outputList<T>(List<T> list)
		{
			foreach (T item in list)
			{
				Console.WriteLine(item);
			}
		}

	}

	[Serializable]
	enum EditionType
	{
		Book,
		Magazine
	}

	[Serializable]
	public class Edition
	{
		public string Name { get; set; }
		public virtual void ParseLine(string line) { }
		public void OnError(string text)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(text);
			Console.ResetColor();
		}
		public void OnAction(string text)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(text);
			Console.ResetColor();
		}
		public override string ToString()
		{
			return $"Name: {Name}";
		}
	}

	[Serializable]
	public class Book : Edition
	{
		public string Author { get; set; }
		public override void ParseLine(string line)
		{
			try
			{
				string[] fields = line.Split(' ');
				Name = fields[0];
				Author = fields[1];
			}
			catch (Exception ex)
			{
				OnError(ex.Message);
			}
		}
		public override string ToString()
		{
			return "Book:\n" + base.ToString() + $" \tAuthor: {Author}";
		}
	}

	[Serializable]
	public class Magazine : Edition
	{
		[XmlArray]
		public List<string> Articles { get; set; } = new List<string>();
		public override void ParseLine(string line)
		{
			try
			{
				string[] fields = line.Split(' ');
				Name = fields[0];
				for (int i = 1; i < fields.Length; i++)
				{
					Articles.Add(fields[i]);
				}
			}
			catch (Exception ex)
			{
				OnError(ex.Message);
			}
		}
		public override string ToString()
		{
			string result = "Magazine:\n" + base.ToString() + "\nArticles:";
			foreach (string article in Articles)
			{
				result += $" {article}";
			}

			return result;
		}
	}
}
