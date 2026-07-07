// Exceed7 behaviour overlay for the DocFX "modern" template.

// ========================================================================
// PRODUCT — edit these per product.
// ========================================================================
const EXC7_GITHUB_REPO = 'https://github.com/5argon/NotchSolution'
const EXC7_DISCORD_URL = 'https://discord.gg/J4sCcj4'
// ========================================================================

// Shared cross-promotion header. Injected on the homepage only, so it stays out of
// the raw Markdown and is identical across every Exceed7 project. Same list everywhere.
const EXC7_CROSS_PROMOTION = `
<div class="fade-in-top-3 cross-promotion">
  <div style="font-size:1rem;">- ASSET STORE -</div>
  <div class="cross-promotion-row">
    <span class="cross-promotion-item"><a href="https://exceed7.com/introloop/" target="_blank">Introloop</a></span>
    <span class="cross-promotion-item"><a href="https://exceed7.com/tiny-ambience/" target="_blank">Tiny Ambience</a></span>
    <!-- Not released yet:
    <span class="cross-promotion-item"><a href="https://exceed7.com/one-shot-framework/" target="_blank">One Shot Framework</a></span>
    <span class="cross-promotion-item"><a href="https://exceed7.com/modular-footstep/" target="_blank">Modular Footstep</a></span>
    -->
  </div>
  <div style="font-size:1rem;">- OPEN SOURCE -</div>
  <div class="cross-promotion-row">
    <span class="cross-promotion-item"><a href="https://exceed7.com/notch-solution/" target="_blank">Notch Solution</a><a class="github-button" href="https://github.com/5argon/NotchSolution" data-icon="octicon-star" data-show-count="true" aria-label="Star 5argon/NotchSolution on GitHub">Star</a></span>
    <span class="cross-promotion-item"><a href="https://github.com/5argon/Minefield" target="_blank">Minefield Test Tools</a><a class="github-button" href="https://github.com/5argon/Minefield" data-icon="octicon-star" data-show-count="true" aria-label="Star 5argon/Minefield on GitHub">Star</a></span>
    <span class="cross-promotion-item"><a href="https://github.com/5argon/NativeAudio" target="_blank">Native Audio</a><a class="github-button" href="https://github.com/5argon/NativeAudio" data-icon="octicon-star" data-show-count="true" aria-label="Star 5argon/NativeAudio on GitHub">Star</a></span>
    <span class="cross-promotion-item"><a href="https://github.com/5argon/protobuf-unity" target="_blank">protobuf-unity</a><a class="github-button" href="https://github.com/5argon/protobuf-unity" data-icon="octicon-star" data-show-count="true" aria-label="Star 5argon/protobuf-unity on GitHub">Star</a></span>
  </div>
</div>`

// True only on the site's index page, regardless of hosting sub-path. The navbar brand
// always links to the site index, so compare the current path against it.
function exc7IsHomepage() {
  const brand = document.querySelector('a.navbar-brand')
  if (!brand) return false
  const norm = (url) => new URL(url).pathname.replace(/index\.html$/, '')
  return norm(brand.href) === norm(location.href)
}

export default {
  defaultTheme: 'auto',
  iconLinks: [
    {
      icon: 'github',
      href: EXC7_GITHUB_REPO,
      title: 'GitHub',
    },
    {
      icon: 'discord',
      href: EXC7_DISCORD_URL,
      title: 'Discord',
    },
  ],
  start: () => {
    // Add a GitHub star button to the navbar.
    const navbar = document.getElementById('navbar')
    if (navbar) {
      const holder = document.createElement('div')
      holder.className = 'exc7-navbar-star'
      holder.innerHTML =
        `<a class="github-button" href="${EXC7_GITHUB_REPO}" data-icon="octicon-star" data-show-count="true" aria-label="Star on GitHub">Star</a>`
      navbar.appendChild(holder)
    }

    // Inject the cross-promotion header at the top of the homepage article.
    if (exc7IsHomepage()) {
      const article = document.querySelector('article')
      if (article) {
        article.insertAdjacentHTML('afterbegin', EXC7_CROSS_PROMOTION)
      }
    }

    // Load the script that turns every .github-button on the page (navbar + homepage
    // header) into a live star button.
    const script = document.createElement('script')
    script.async = true
    script.defer = true
    script.src = 'https://buttons.github.io/buttons.js'
    document.body.appendChild(script)
  },
}
