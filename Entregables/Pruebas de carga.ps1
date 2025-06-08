Write-Host "Iniciando prueba de carga en http://localhost/api/tareas"
Write-Host "Presiona Ctrl+C para detener."

$i = 0
while ($true) {
    try {

        $duration = Measure-Command {    
            Invoke-WebRequest -Uri "http://localhost/api/tareas" -Method Get -UseBasicParsing | Out-Null
        }
        Invoke-WebRequest -Uri "http://localhost/api/tareas" -Method Get -UseBasicParsing | Out-Null
        $i++
        Write-Host "Petición #$i tardó $($duration.TotalMilliseconds) ms"
        Write-Host "`r" -NoNewline
    }
    catch {
        Write-Host "Error en la petición: $_" -ForegroundColor Red
    }    
    Start-Sleep -Milliseconds 400
}
