# (c) 2019 Max Feingold

function Increment-CsprojVersionWithPattern
{
    Param([string]$path, [string]$pattern, [string]$element, [bool]$four=$false)

    (Get-Content $path) | ForEach-Object {

        if (!($_.ToString().StartsWith('//')) -and ($_ -match $pattern))
        {
            $prefix = $_.ToString().Substring(0, $_.ToString().IndexOf($matches[0]))
            $version = [version]$matches[1]

            if ($four)
            {
                ($prefix + '<' + $element + '>{0}.{1}.{2}.0</' + $element + '>') -f $version.Major, $version.Minor, ($version.Build + 1)
            }
            else
            {
                ($prefix + '<' + $element + '>{0}.{1}.{2}</' + $element + '>') -f $version.Major, $version.Minor, ($version.Build + 1)
            }
        }
        else
        {
            # Output line as is
            $_
        }

    } | Set-Content $path
}

function Increment-CsprojVersion
{
    Param([string]$path)

    Increment-CsprojVersionWithPattern -path $path -element 'Version' -pattern '\<Version\>(.*)\<\/Version\>'
}

Increment-CsprojVersion -path (Join-Path $pwd 'Source/HtmlTextWriter/HtmlTextWriter.csproj')
