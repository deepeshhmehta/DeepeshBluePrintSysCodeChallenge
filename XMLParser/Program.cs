using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;

namespace XMLParser
{
    class Program
    {
        // Path where XML exists and JSON will be written
        public static String path = "/Users/deepeshmehta/Work/Personal/blue-print/XMLParser/XMLParser/";
        public static void Main(string[] args)
        {
            // For clear output
            Console.Clear();
            Console.WriteLine("\n-------\n");

            // Read the XML
            XmlDocument doc = new XmlDocument();
            doc.Load(Program.path + "organization.xml");

            // List that will be used to dump data in console
            List<Person> people = new List<Person>();
            Unit objectRepresentation = new Unit();

            // Traverse through the XML and extract required data by reference
            Program.traverseXMLandGenerateListAndObjectRepresentation(doc.DocumentElement.ChildNodes, ref people, ref objectRepresentation);

            // Dump people data to console
            Program.printListToConsole(people);

            // Write Json To File
            Program.writeJsonToFile(objectRepresentation, "BeforeSwap.json");

            // Flip departments
            Program.switchDepartments(ref objectRepresentation);

            // Write Json To File
            Program.writeJsonToFile(objectRepresentation, "AfterSwap.json");

        }

        static void traverseXMLandGenerateListAndObjectRepresentation(XmlNodeList nodes, ref List<Person> people, ref Unit objectRepresentation)
        {
            // Traverse over each node
            foreach (XmlNode node in nodes)
            {
                // name of the unit is the name of the parent node
                String unitName = (node.ParentNode.Attributes["Name"])?.InnerText;

                // if child node is 1, it is an employee entity
                if (node.ChildNodes.Count == 1)
                {
                    String name = node.InnerText;
                    String title = (node.Attributes["Title"])?.InnerText;

                    Person person = new Person();
                    person.name = name;
                    person.title = title;
                    person.unitName = unitName;

                    // add to list to console dump
                    people.Add(person);

                    // add to object for json representation
                    objectRepresentation.employees.Add(person);
                    objectRepresentation.name = unitName;
                }
                // if child is not 1 another units await
                else {
                    // generate a unit and assign to the list
                    objectRepresentation.name = unitName;
                    Unit unit = new Unit();
                    // recursive call to traverse through each sub-unit
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

            // Extract respective departments from the object
            Program.getTeams(ref platformTeam, ref maintenanceTeam, objectRepresentation);

            // Swap
            List<Employee> temp = platformTeam.employees;
            platformTeam.employees = maintenanceTeam.employees;
            maintenanceTeam.employees = temp;

        }

        static void getTeams(ref Unit platformTeam, ref Unit maintenanceTeam, Unit traverseObject)
        {
            // If current team is platform-team extract object by reference
            if (traverseObject.name == "Platform Team")
            {
                platformTeam = traverseObject;
            }
            // If current team is maintenance-team extract object by reference
            else if (traverseObject.name == "Maintenance Team")
            {
                maintenanceTeam = traverseObject;
            }
            // traverse recursively through all sub-units to find team
            else
            {
                for(int i=0; i < traverseObject.units.Count; i++)
                {
                    var unit = traverseObject.units[i];
                    Program.getTeams(ref platformTeam, ref maintenanceTeam, unit);
                }
            }
        }

        static async void writeJsonToFile(Unit objectRepresentation, String fileName) {
            String jsonRepresentation = objectRepresentation.toString();
            await File.WriteAllTextAsync(Program.path + fileName, jsonRepresentation);
        }

    }

    // Employee entity
    class Employee
    {
        public String title;
        public String name;

        // String representation of employee for JSON
        public String toString() {
            return "{\"title\":\""+ this.title+ "\",\"name\":\"" + this.name + "\"}";
        }
    }

    // Person extends employee
    class Person : Employee
    {
        public String unitName;
    }

    // Unit entity
    class Unit
    {
        public String name;
        public List<Employee> employees = new List<Employee>();
        public List<Unit> units = new List<Unit>();

        // String representation of unit for JSON
        public String toString()
        {
            // name
            String name = "{\"name\":\"" + this.name + "\",";

            // open employees array
            String employees = "\"employees\":[";

            // traverse through each employee and extract the string representation
            for( int i=0; i < this.employees.Count; i++) {
                employees += this.employees[i].toString();
                if (i!= this.employees.Count -1)
                {
                    employees += ",";
                }
            };
            // Close the employees array
            employees += "],";

            // open units array
            String units = "\"units\":[";
            for (int i=0; i < this.units.Count; i++)
            {
                // call each unit's toString (current) function
                units += this.units[i].toString();
                if (i != this.units.Count - 1)
                {
                    units+= ",";
                }
            }
            // close array
            units += "]\n}";

            // combine 3 fields for each unit and return a string
            return name + employees + units;
        }
    }
}
