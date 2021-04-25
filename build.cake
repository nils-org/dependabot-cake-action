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
var testNoDryRun = Argument<bool>("test-no-dryrun", false);

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
   if(string.IsNullOrEmpty(EnvironmentVariable("INPUT_TOKEN")))
   {
      throw new ArgumentException("'INPUT_TOKEN' not set. Please set INPUT_TOKEN to your GitHub pat");
   } 

   var envArgs = new List<string>
   { 
      $"GITHUB_REPOSITORY={testRepositoryName}",
      $"INPUT_TARGET_BRANCH={testRepositoryBranch}",
      "INPUT_TOKEN",
   };

   if (!testNoDryRun)
   {
      envArgs.Add("DRY_RUN=1");
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
