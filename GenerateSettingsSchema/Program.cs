
using FileSorter;
using NJsonSchema;

var schema = JsonSchema.FromType<Settings>();
//var errors = schema.Validate("{...}");

//foreach (var error in errors)
//    Console.WriteLine(error.Path + ": " + error.Kind);

await File.WriteAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "schema.json"), schema.ToJson());