#!/usr/bin/env bash
#
# Build and preview the Notch Solution documentation site with DocFX.
#
# One-time prerequisites:
#   1. Install the .NET SDK (8.x or 9.x): https://dotnet.microsoft.com/download
#   2. Install DocFX as a global tool:    dotnet tool update -g docfx
#   3. Open src/NotchSolution in Unity once so it generates the .sln / .csproj
#      that the API-metadata step compiles against.
#
# Usage:
#   ./build-docs.sh           Full build (API metadata + site) and serve at http://localhost:8080
#   ./build-docs.sh --build   Full build only, no server
#   ./build-docs.sh --fast    Build the site WITHOUT regenerating API docs, then serve.
#                             Much faster — use it for CSS / Markdown / TOC edits.
#   ./build-docs.sh --watch   Like --fast, but rebuild automatically whenever a doc or
#                             template file changes.
#
# DocFX has no live reload: after each rebuild, hard-refresh the browser (Cmd+Shift+R),
# because the template does not cache-bust main.css / main.js.
#
set -euo pipefail

repo_root="$(cd "$(dirname "$0")" && pwd)"
config="$repo_root/.docfx_project/docfx.json"
site="$repo_root/.docfx_project/_site"

watch_dirs=(
  "$repo_root/.docfx_project"
  "$repo_root/src/NotchSolution/Assets/NotchSolution/Documentation~"
  "$repo_root/src/NotchSolution/Assets/NotchSolution/CHANGELOG.md"
)

if ! command -v docfx >/dev/null 2>&1; then
  echo "docfx not found. Install it with: dotnet tool update -g docfx" >&2
  exit 1
fi

case "${1:-}" in
  --build)
    docfx "$config"
    ;;
  --fast)
    # "docfx build" runs only the build step, skipping the slow API-metadata compile.
    docfx build "$config" --serve
    ;;
  --watch)
    docfx build "$config"
    docfx serve "$site" &
    server_pid=$!
    trap 'kill "$server_pid" 2>/dev/null || true' EXIT
    echo "Watching for changes (1s poll) — hard-refresh the browser after each rebuild. Ctrl+C to stop."

    # Hash the CONTENT of source files only. Generated output is pruned (_site, obj,
    # api) and — crucially — the baseline is re-taken AFTER every build, so anything a
    # build writes can never re-trigger the watcher. Only a real edit made after a
    # build causes the next rebuild.
    hash_sources() {
      find "${watch_dirs[@]}" \
          -type d \( -name _site -o -name obj -o -name .git -o -name api \) -prune -o \
          -type f \( -name '*.md' -o -name '*.yml' -o -name '*.yaml' -o -name '*.css' \
                     -o -name '*.js' -o -name '*.json' -o -name '*.png' -o -name '*.gif' -o -name '*.jpg' \) -print \
          2>/dev/null | sort | xargs md5 2>/dev/null | md5 2>/dev/null || true
    }

    last="$(hash_sources)"
    while true; do
      sleep 1
      now="$(hash_sources)"
      if [ -n "$now" ] && [ "$now" != "$last" ]; then
        docfx build "$config" || true
        last="$(hash_sources)"
      fi
    done
    ;;
  "")
    docfx "$config" --serve
    ;;
  *)
    echo "Unknown option: ${1}" >&2
    echo "Use: (no arg) | --build | --fast | --watch" >&2
    exit 1
    ;;
esac
