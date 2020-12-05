using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace RotMGServerChanger {
    
    static class Program {

        private static ConsoleColor DefaultConsoleColor;
        
        private static void Main(string[] args){
            DefaultConsoleColor = Console.ForegroundColor;
            bool isProd = IsServerProd();
            List<string> servers = GetAllServers(isProd);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Available Servers:");
            Console.ForegroundColor = ConsoleColor.Green;
            for (int i = 0; i < servers.Count; i++) {
                Console.WriteLine($"{i+1}. {servers[i]}");
            }
            Console.ForegroundColor = DefaultConsoleColor;
            string serverName = GetValidServer(servers);
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DECA Live Operations GmbH\RotMGExalt");
            string[] names = key.GetValueNames();
            foreach (string name in names) {
                if (name.StartsWith("preferredServer")) {
                    string server = serverName + "\u0000";
                    byte[] unicodeBytes = Encoding.UTF8.GetBytes(server);
                    key.SetValue(name, unicodeBytes);
                }
            }
            key.Close();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Server Set! Press <enter> to exit.");
            Console.ForegroundColor = DefaultConsoleColor;
            Console.ReadLine();
        }
        
        private static List<string> GetAllServers(bool prod){
            XmlDocument doc = new XmlDocument();
            string baseUrl = prod ? "realmofthemadgodhrd" : "rotmgtesting";
            doc.Load($"http://{baseUrl}.appspot.com/char/list");
            doc.Normalize();
            XmlElement serversElem = doc.DocumentElement.GetChildElement("Servers");
            List<string> servers = new List<string>();
            foreach (XmlNode docChildNode in serversElem.ChildNodes) {
                if (docChildNode is XmlElement elem) {
                    if (elem.Name == "Server") {
                        servers.Add(GetServerName(elem));
                    }
                }
            }
            return servers;
        }

        private static string GetServerName(XmlElement elem){
            return GetChildElement(elem, "Name").InnerText;
        }

        private static XmlElement GetChildElement(this XmlElement elem, string childName){
            foreach (XmlNode docChildNode in elem.ChildNodes) {
                if (docChildNode is XmlElement child) {
                    if (child.Name == childName) {
                        return child;
                    }
                }
            }
            return null;
        }

        private static bool IsServerProd(){
            Console.Write("Production or Testing (P/T)? ");
            string val = Console.ReadLine();
            val = val?.ToLower();
            if (string.IsNullOrEmpty(val) || (val[0] != 'p' && val[0] != 't')) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid server type");
                Console.ForegroundColor = DefaultConsoleColor;
                return IsServerProd();
            }

            bool ret = val[0] == 'p';
            Console.WriteLine("Using " + (ret ? "Production" : "Testing") + " servers");
            return ret;
        }
        
        private static string GetValidServer(List<string> servers){
            Console.Write($"What server do you want to set (1-{servers.Count})? ");
            string val = Console.ReadLine();
            if (!string.IsNullOrEmpty(val) && Int32.TryParse(val, out int serverId)) {
                if (serverId > 0 && serverId <= servers.Count) {
                    string server = servers[serverId - 1];
                    Console.Write("Setting server to ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(server);
                    Console.ForegroundColor = DefaultConsoleColor;
                    Console.WriteLine("...");
                    return server;
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid server number");
            Console.ForegroundColor = DefaultConsoleColor;
            return GetValidServer(servers);
        }
    }
}