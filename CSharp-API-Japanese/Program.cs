using CSharp_API_Japanese;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;


string libFolderPath = @"D:\Data\Dictionary\Japanese\";
string jmdict_m_filename = @"JMdict.xml";
string jmdict_e_filename = @"JMdict_e.xml";
string edict2_filename = @"edict2";
string jmdict_Path = System.IO.Path.Combine(libFolderPath, jmdict_e_filename);
string edict2_Path = System.IO.Path.Combine(libFolderPath, edict2_filename);

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("\n日本語 API 🐱\n");

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Parse command-line args
string mode = args.FirstOrDefault(a => a.StartsWith("--mode="))?.Split("=")[1]?.ToLower() ?? "edict2";

mode = "edict2";
//IApiModule api = useJmdict ? new JMDictApi(jmdictPath) : new EDict2Api(edictPath);
//api.RegisterRoutes(app);


IApiModule api;



if (mode == "edict2")
{
    api = new EDict2Api(edict2_Path);
}
else
{
    api = new JMDictApi(jmdict_Path);
}

// Register routes dynamically
api.RegisterRoutes(app);

app.Run();
