dotnet test /p:CollectCoverage=true /p:Exclude=\"[Community.Archives.Core.Tests]*\" /p:CoverletOutput=../CoverageResults/ /p:MergeWith="../CoverageResults/coverage.json" /p:CoverletOutputFormat=\"cobertura,json\" /p:ThresholdType=\"line,branch,method\" -m:1

rmdir /S /Q CoverageReport
reportgenerator "-reports:.\CoverageResults\coverage.cobertura.xml" "-targetdir:CoverageReport" "-reporttypes:Html;HtmlSummary"
rmdir /S /Q CoverageResults
