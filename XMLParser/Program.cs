using System;
using System.Xml;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace XMLParser
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("\n-------\n");

            XmlDocument doc = new XmlDocument();
            doc.Load("/Users/deepeshmehta/Work/Personal/blue-print/XMLParser/XMLParser/organization.xml");

            List<Person> people = new List<Person>();
            Unit objectRepresentation = new Unit();

            Program.traverseXMLandGenerateListAndObjectRepresentation(doc.DocumentElement.ChildNodes, ref people, ref objectRepresentation);

            Program.printListToConsole(people);

            Program.switchDepartments(ref objectRepresentation);

            Program.writeJsonToFile(objectRepresentation);

        }

        static void traverseXMLandGenerateListAndObjectRepresentation(XmlNodeList nodes, ref List<Person> people, ref Unit objectRepresentation)
        {
            foreach (XmlNode node in nodes)
            {
                String unitName = (node.ParentNode.Attributes["Name"])?.InnerText;
                if (node.ChildNodes.Count == 1)
                {
                    String name = node.InnerText;
                    String title = (node.Attributes["Title"])?.InnerText;

                    Person person = new Person();
                    person.name = name;
                    person.title = title;
                    person.unitName = unitName;

                    people.Add(person);
                    objectRepresentation.employees.Add(person);
                    objectRepresentation.name = unitName;
                }
                else {
                    objectRepresentation.name = unitName;
                    Unit unit = new Unit();
                    Program.traverseXMLandGenerateListAndObjectRepresentation(node.ChildNodes, ref people, ref unit);
                    objectRepresentation.units.Add(unit);
                }
            }
        }

        static void printListToConsole(List<Person> people)
        {
            people.ForEach(person =>
            {
                String dump = "Name - " + person.name + ", Title - " + person.title + ", Unit Name - " + person.unitName;
                Console.WriteLine(dump);
            });
        }

        static void switchDepartments(ref Unit objectRepresentation)
        {
            Unit platformTeam = new Unit();
            Unit maintenanceTeam = new Unit();
            Program.getTeams(ref platformTeam, ref maintenanceTeam, objectRepresentation);
            List<Employee> temp = platformTeam.employees;
            platformTeam.employees = maintenanceTeam.employees;
            maintenanceTeam.employees = temp;

        }

        static void getTeams(ref Unit platformTeam, ref Unit maintenanceTeam, Unit traverseObject)
        {
            if (traverseObject.name == "Platform Team")
            {
                platformTeam = traverseObject;
            }
            else if (traverseObject.name == "Maintenance Team")
            {
                maintenanceTeam = traverseObject;
            }
            else
            {
                for(int i=0; i < traverseObject.units.Count; i++)
                {
                    var unit = traverseObject.units[i];
                    Program.getTeams(ref platformTeam, ref maintenanceTeam, unit);
                }
            }
        }

        static async void writeJsonToFile(Unit objectRepresentation) {
            String jsonRepresentation = objectRepresentation.toString();
            await File.WriteAllTextAsync("/Users/deepeshmehta/Work/Personal/blue-print/XMLParser/XMLParser/WriteText.json", jsonRepresentation);
        }

    }


    class Employee
    {
        public String title;
        public String name;
        public String toString() {
            return "{\"title\":\""+ this.title+ "\",\"name\":\"" + this.name + "\"}";
        }
    }

    class Person : Employee
    {
        public String unitName;
    }

    class Unit
    {
        public String name;
        public List<Employee> employees = new List<Employee>();
        public List<Unit> units = new List<Unit>();

        public String toString()
        {
            String name = "{\"name\":\"" + this.name + "\",";

            String employees = "\"employees\":[";
            
            for( int i=0; i < this.employees.Count; i++) {
                employees += this.employees[i].toString();
                if (i!= this.employees.Count -1)
                {
                    employees += ",";
                }
            };
            employees += "],";

            String units = "\"units\":[";
            for (int i=0; i < this.units.Count; i++)
            {
                units += this.units[i].toString();
                if (i != this.units.Count - 1)
                {
                    units+= ",";
                }
            }
            units += "]}";

            return name + employees + units;
        }
    }
}
