using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    abstract class Element
    {
        protected char id;
        protected int enters;
        protected char[] connectedElements;
        protected byte[] values;
        protected byte result;

        protected abstract void Calculate();

        public Element(char id, int enters, char[] connectedElements, byte[] values)
        {
            this.id = id;
            this.enters = enters;
            this.connectedElements = connectedElements;
            this.values = values;
        }

        public byte GetResult()
        {
            Calculate();
            Console.WriteLine("Result: {0}", result);
            return result;
        }

        public char GetId()
        {
            Console.WriteLine("Id: {0}", id);
            return id;
        }

        public int GetEnters()
        {
            Console.WriteLine("Enters: {0}", enters);
            return enters;
        }

        public char[] GetConnectedElements()
        {
            Console.WriteLine("Connected elements: ");

            for (int i = 0; i < connectedElements.Length; i++)
            {
                Console.WriteLine("Element id #{0}: {1}", i + 1, connectedElements[i]);
            }

            return connectedElements;
        }

        public byte[] GetValues()
        {
            Console.WriteLine("Values: ");

            for (int i = 0; i < values.Length; i++)
            {
                Console.WriteLine("Value #{0}: {1}", i + 1, values[i]);
            }

            return values;
        }

        public Element GetInfo()
        {
            GetId();
            GetEnters();
            GetConnectedElements();
            GetValues();
            GetResult();
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Element objct = obj as Element;

            if (id == objct.id &&
                enters == objct.enters &&
                result == objct.result &&
                connectedElements.Length == objct.connectedElements.Length &&
                values.Length == objct.values.Length)
            {
                var current = new SortedSet<char>(connectedElements);
                var compared = new SortedSet<char>(objct.connectedElements);

                if (current.Count() != compared.Count())
                {
                    return false;
                }

                var currentToCompared = current.Zip(compared, (cr, cm) => new { Current = cr, Compared = cm });

                foreach (var cc in currentToCompared)
                {
                    if (cc.Compared != cc.Current)
                    {
                        return false;
                    }
                }

                if (values.Where(x => x == 1).Count() != objct.values.Where(x => x == 1).Count())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = id;

            hashCode += GetType().Name.GetHashCode();

            foreach (var v in values)
            {
                hashCode += 2 * v + 4 * Math.Abs(v - 1);
            }

            return hashCode;
        }

        public static bool operator ==(Element compared, Element current)
        {
            if (compared.Equals(current))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(Element compared, Element current)
        {
            if (!compared.Equals(current))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Element DeepCopy()
        {
            if (GetType().Name == "AND_NOT")
            {
                return new AND_NOT(id, enters, connectedElements, values);
            }
            else
            {
                return new OR_NOT(id, enters, connectedElements, values);
            }
        }
    }

    class AND_NOT : Element
    {
        public AND_NOT(char id, int enters, char[] connectedElements, byte[] values) : base(id, enters, connectedElements, values) { }

        protected override void Calculate()
        {
            result = 0;
            int i = 0;
            while (i < values.Length)
            {
                if (values[i] == 0)
                {
                    result = 1;
                    return;
                }

                i++;
            }
        }
    }

    class OR_NOT : Element
    {
        public OR_NOT(char id, int enters, char[] connectedElements, byte[] values) : base(id, enters, connectedElements, values) { }

        protected override void Calculate()
        {
            result = 1;
            int i = 0;
            while (i < values.Length)
            {
                if (values[i] == 1)
                {
                    result = 0;
                    return;
                }

                i++;
            }
        }
    }

    class Schema
    {
        List<Element> elements;

        public delegate void Delete(int index);
        public delegate void Add(char id);
        public event Delete OnDelete;
        public event Add OnAdd;

        public Schema(params Element[] elements)
        {
            this.elements = new List<Element>();
            this.elements.AddRange(elements);
        }

        public override string ToString()
        {
            StringBuilder info = new StringBuilder("Number of elements: " + elements.Count() + "\n");

            foreach (Element element in elements)
            {
                info.Append("Type: ");
                info.Append(element);
                info.Append("\n");

                info.Append("Id: ");
                info.Append(element.GetId());
                info.Append("\n");

                info.Append("Result: ");
                info.Append(element.GetResult());
                info.Append("\n");

                info.Append("Enters: ");
                info.Append(element.GetEnters());
                info.Append("\n");

                info.Append("Connected elements: ");
                info.Append(element.GetConnectedElements());
                info.Append("\n");

                info.Append("_____________\n");
                //result += element + "============\n";
            }

            Console.WriteLine("Info about elements: {0}", info);

            return info.ToString();
        }

        public void DeleteAt(int index)
        {
            if (index < elements.Count)
            {
                elements.RemoveAt(index);
            }
            else throw new IndexOutOfRangeException();
            OnDelete(index);
        }

        public void AddToList(Element element)
        {
            elements.Add(element);

            OnAdd(element.GetId());
        }
    }

    class EventHandler
    {
        public void Deleted(int index)
        {
            Console.WriteLine("The element at index {0} was removed.", index);
        }

        public void Added(char id)
        {
            Console.WriteLine("The element with id {0} has been added to the list...", id);
        }
    }

    public class StackOverflowException : Exception
    {
        public StackOverflowException()
            : base("Overflow") { }
    }

    public class ArrayTypeMismatchException : Exception
    {
        public ArrayTypeMismatchException()
            : base("There are different types") { }
    }

    public class DivideByZeroException : Exception
    {
        public DivideByZeroException()
            : base("Dividing by zero - impossible") { }
    }

    public class IndexOutOfRangeException : Exception
    {
        public IndexOutOfRangeException()
            : base("Index is out of range") { }
    }

    public class InvalidCastException : Exception
    {
        public InvalidCastException()
            : base("Invalid cast") { }
    }

    public class OutOfMemoryException : Exception
    {
        public OutOfMemoryException()
            : base("Not enough memory") { }
    }

    public class OverflowException : Exception
    {
        public OverflowException()
            : base("Overflow, too much operations") { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var element0 = new AND_NOT('d', 10, new char[] { 'a', 'b', 'k' }, new byte[] { 1, 1, 1 });
                element0.GetInfo();
                Console.WriteLine("Hash code: {0}", element0.GetHashCode());

                var element1 = new AND_NOT('w', 10, new char[] { 'q', 'r', 'b' }, new byte[] { 0, 1, 1 });

                var sch = new Schema(element0, element1);
                sch.ToString();

                if (element0.Equals(element1))
                {
                    Console.WriteLine("These elements are equal");
                }
                else
                {
                    Console.WriteLine("These elements aren't equal");
                }

                if (element0 != element1)
                {
                    Console.WriteLine("These elements aren't the same");
                }
                else
                {
                    Console.WriteLine("These elements are the same");
                }

                var element2 = new OR_NOT('d', 10, new char[] { 'a', 'b', 'k' }, new byte[] { 1, 1, 1 });
                var element3 = element2.DeepCopy();

                if (element2 == element3)
                {
                    Console.WriteLine("These elements are the same");
                }
                else
                {
                    Console.WriteLine("These elements aren't the same");
                }

                var eventHandler = new EventHandler();


                sch.OnDelete += eventHandler.Deleted;
                sch.OnAdd += eventHandler.Added;

                sch.AddToList(element3);
                sch.DeleteAt(5);

                //for check:

                //throw new OverflowException();
                //throw new OutOfMemoryException();
                //throw new InvalidCastException();
                //throw new DivideByZeroException();
                //throw new ArrayTypeMismatchException();
                //throw new StackOverflowException();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadKey();

            
        }
    }
}
