@if "%SCM_TRACE_LEVEL%" NEQ "4" @echo off

IF "%SITE_ROLE%" == "bot" (
  deploy.bot.cmd
) ELSE (
  IF "%SITE_ROLE%" == "config" (
    deploy.config.cmd
  ) ELSE (
    echo You have to set SITE_ROLE setting to either "bot" or "config"
    exit /b 1
  )
)