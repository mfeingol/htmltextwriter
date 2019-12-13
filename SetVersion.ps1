param([Parameter(Mandatory=$true)][string]$version)

function Set-CsprojVersionWithPattern
{
    Param([string]$version, [string]$path, [string]$pattern, [string]$element, [bool]$four=$false)

    (Get-Content $path) | ForEach-Object {

        if (!($_.ToString().StartsWith('//')) -and ($_ -match $pattern))
        {
            $prefix = $_.ToString().Substring(0, $_.ToString().IndexOf($matches[0]))

            if ($four)
            {
                ($prefix + '<' + $element + '>' + $version + '.0' + '</' + $element + '>')
            }
            else
            {
                ($prefix + '<' + $element + '>' + $version + '</' + $element + '>')
            }
        }
        else
        {
            # Output line as is
            $_
        }

    } | Set-Content $path
}

function Set-CsprojVersion
{
    Param([string]$version, [string]$path)

    Set-CsprojVersionWithPattern -version $version -path $path -element 'Version' -pattern '\<Version\>(.*)\<\/Version\>'
}

Set-CsprojVersion -version $version -path (Join-Path $pwd 'Source/System.Web.UI.HtmlTextWriter/System.Web.UI.HtmlTextWriter.csproj')
