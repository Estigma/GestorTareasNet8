#!/bin/bash
# Esperar a que SQL Server esté listo
sleep 30s

# Ejecutar script SQL para crear la base de datos
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Salesiana.2025 -d master -i /usr/src/init.sql