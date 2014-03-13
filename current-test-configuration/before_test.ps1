$dbUid = $env:DB_UID
if($dbUid -eq $null) {
    $dbUid = $env:USERNAME
}
$dbUid = $dbUid.replace('-', '_')

$dbuserName = "nhibernate_$dbUid"

# subst test user into test cfg
	$path = "$(Get-Location)\hibernate.cfg.xml"
    [xml]$nhCfg = Get-Content $path
    $nhCfg."hibernate-configuration"."session-factory"."property"[0].InnerText = "User ID=$dbuserName; password=nhibernate; data source=evol10"
    $nhCfg.Save($path)