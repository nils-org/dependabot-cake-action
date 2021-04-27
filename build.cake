#tool dotnet:?package=GitVersion.Tool&version=5.6.8
#addin nuget:?package=Cake.Docker&version=1.0.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var imageName = Argument("imageName", "dependabot-cake");

// test
var testRepositoryName = Argument("test-RepositoryName", "nils-a/Cake.7zip");
var testRepositoryBranch = Argument("test-RepositoryBranch", "develop");
var testFolders = Arguments<string>("test-folder", null).Where(x => x != null).ToList();
var testIgnore = Arguments<string>("test-ignore", null).Where(x => x != null).ToList();
var testNoDryRun = HasArgument("test-no-dryrun");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

var imageFullTag = "";
Task("Calculate-Image-Tag")
   .Does(() =>
{
   var version = GitVersion();
   imageFullTag = $"{imageName}:{version.SemVer}";
   Information($"calculated tag for image: {imageFullTag}");
});


Task("Build-Image")
   .IsDependentOn("Calculate-Image-Tag")
   .Does(() =>
{
   DockerBuild(new DockerImageBuildSettings 
   {
      Tag = new[] {imageFullTag},
   }, "src");
});

Task("Run-Test")
   .IsDependentOn("Calculate-Image-Tag")
   .IsDependentOn("Build-Image")
   .Does(() =>
{
   Information($"running test on RepositoryName:{testRepositoryName} branch:{testRepositoryBranch}");
   if(testFolders.Count > 0)
   { 
       Information($"searching in folders: {string.Join(", ", (IEnumerable<string>)testFolders)}");
   } 
   if(testIgnore.Count > 0)
   { 
       Information($"ignoring packages: {string.Join(", ", (IEnumerable<string>)testIgnore)}");
   } 
   if(testNoDryRun)
   {
      Warning("NO-DRY-RUN is set. Real PRs will be created.");
   } 

   var branches = string.Join("\n", testRepositoryBranch);

   var envArgs = new List<string>
   { 
      $"GITHUB_REPOSITORY={testRepositoryName}",
      $"INPUT_TARGET_BRANCH={branches}",
      "INPUT_TOKEN",
   };

   if (testFolders.Count > 0)
   {
      envArgs.Add($"INPUT_DIRECTORY={string.Join("\n", (IEnumerable<string>)testFolders)}");
   } 

   if (!testNoDryRun)
   {
      envArgs.Add("DRY_RUN=1");
   } 

   if(testIgnore.Count > 0)
   {
      envArgs.Add($"INPUT_IGNORE={string.Join("\n", (IEnumerable<string>)testIgnore)}");
   } 

   if(string.IsNullOrEmpty(EnvironmentVariable("INPUT_TOKEN")))
   {
      throw new ArgumentException("'INPUT_TOKEN' not set. Please set INPUT_TOKEN to your GitHub pat");
   } 

   DockerRunWithoutResult(new DockerContainerRunSettings
   {
      Env = envArgs.ToArray(),
      Rm = true,
   },
   imageFullTag,
   "");
});



Task("Default")
.Does(() => {
   Information($"test no dry-run is: {testNoDryRun}");
   Warning("Currently there is no default. Chose a better target!");
});

RunTarget(target);
