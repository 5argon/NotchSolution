#!/usr/bin/env bash
#
# Build this package's documentation site.
#
# The DocFX configuration and the Exceed7 theme are NOT in this repository — they live
# in the Exceed7Website repo, which generates a config for this package at build time.
# That is why a font change or a header-bar change is one edit there rather than one
# commit in every package repo.
#
# This script is identical in every package repo: it derives the package name from its
# own directory, so there is nothing here to keep in sync.
#
# Requires: the Exceed7Website repo checked out next to the package repos, node, and
# docfx (dotnet tool update -g docfx).
#
# Output lands in Exceed7Website/dist/<slug>/.
#
set -euo pipefail

repo_root="$(cd "$(dirname "$0")" && pwd)"
package_name="$(basename "$repo_root")"

# Package repos sit in a flat fleet directory; Exceed7Website is a sibling of that
# directory, so it is two levels up. EXCEED7_SITE_REPO overrides for other layouts.
site_repo="${EXCEED7_SITE_REPO:-$repo_root/../../Exceed7Website}"

if [ ! -f "$site_repo/scripts/assemble.mjs" ]; then
  echo "Cannot find Exceed7Website (looked in $site_repo)." >&2
  echo "Clone it beside the fleet directory, or set EXCEED7_SITE_REPO." >&2
  exit 1
fi

exec node "$site_repo/scripts/assemble.mjs" --only "$package_name" --skip-site "$@"
