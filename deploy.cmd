@if "%SCM_TRACE_LEVEL%" NEQ "4" @echo off

IF "%SITE_KIND%" == "bot" (
  deploy.bot.cmd
) ELSE (
  IF "%SITE_KIND%" == "config" (
    deploy.config.cmd
  ) ELSE (
    echo You have to set SITE_KIND setting to either "bot" or "config"
    exit /b 1
  )
)