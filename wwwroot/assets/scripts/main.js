/**
* Template Name: Nova
* Template URL: https://bootstrapmade.com/nova-bootstrap-business-template/
* Updated: Aug 07 2024 with Bootstrap v5.3.3
* Author: BootstrapMade.com
* License: https://bootstrapmade.com/license/
*/

(function() {
  "use strict";

  /**
   * Apply .scrolled class to the body as the page is scrolled down
   */
  function toggleScrolled() {
    const selectBody = document.querySelector('body');
    const selectHeader = document.querySelector('#header');
    if (!selectHeader.classList.contains('scroll-up-sticky') && !selectHeader.classList.contains('sticky-top') && !selectHeader.classList.contains('fixed-top')) return;
    window.scrollY > 100 ? selectBody.classList.add('scrolled') : selectBody.classList.remove('scrolled');
  }

  document.addEventListener('scroll', toggleScrolled);
  window.addEventListener('load', toggleScrolled);

  /**
   * Mobile nav toggle
   */
  const mobileNavToggleBtn = document.querySelector('.mobile-nav-toggle');

  function mobileNavToogle() {
    document.querySelector('body').classList.toggle('mobile-nav-active');
    mobileNavToggleBtn.classList.toggle('bi-list');
    mobileNavToggleBtn.classList.toggle('bi-x');
  }
  mobileNavToggleBtn.addEventListener('click', mobileNavToogle);

  /**
   * Hide mobile nav on same-page/hash links
   */
  document.querySelectorAll('#navmenu a').forEach(navmenu => {
    navmenu.addEventListener('click', () => {
      if (document.querySelector('.mobile-nav-active')) {
        mobileNavToogle();
      }
    });

  });

  /**
   * Toggle mobile nav dropdowns
   */
  document.querySelectorAll('.navmenu .toggle-dropdown').forEach(navmenu => {
    navmenu.addEventListener('click', function(e) {
      e.preventDefault();
      this.parentNode.classList.toggle('active');
      this.parentNode.nextElementSibling.classList.toggle('dropdown-active');
      e.stopImmediatePropagation();
    });
  });

  /**
   * Preloader
   */
  const preloader = document.querySelector('#preloader');
  if (preloader) {
    window.addEventListener('load', () => {
      preloader.remove();
    });
  }

  /**
   * Scroll top button
   */
  let scrollTop = document.querySelector('.scroll-top');

  function toggleScrollTop() {
    if (scrollTop) {
      window.scrollY > 100 ? scrollTop.classList.add('active') : scrollTop.classList.remove('active');
    }
  }
  scrollTop.addEventListener('click', (e) => {
    e.preventDefault();
    window.scrollTo({
      top: 0,
      behavior: 'smooth'
    });
  });

  window.addEventListener('load', toggleScrollTop);
  document.addEventListener('scroll', toggleScrollTop);

  /**
   * Animation on scroll function and init
   */
  function aosInit() {
    AOS.init({
      duration: 600,
      easing: 'ease-in-out',
      once: true,
      mirror: false
    });
  }
  window.addEventListener('load', aosInit);

  /**
   * Initiate glightbox
   */
  const glightbox = GLightbox({
    selector: '.glightbox'
  });

  /**
   * Init swiper sliders
   */
  function initSwiper() {
    document.querySelectorAll(".init-swiper").forEach(function(swiperElement) {
      let config = JSON.parse(
        swiperElement.querySelector(".swiper-config").innerHTML.trim()
      );

      if (swiperElement.classList.contains("swiper-tab")) {
        initSwiperWithCustomPagination(swiperElement, config);
      } else {
        new Swiper(swiperElement, config);
      }
    });
  }

  window.addEventListener("load", initSwiper);

  /**
   * Init isotope layout and filters
   */
  document.querySelectorAll('.isotope-layout').forEach(function(isotopeItem) {
    let layout = isotopeItem.getAttribute('data-layout') ?? 'masonry';
    let filter = isotopeItem.getAttribute('data-default-filter') ?? '*';
    let sort = isotopeItem.getAttribute('data-sort') ?? 'original-order';

    let initIsotope;
    imagesLoaded(isotopeItem.querySelector('.isotope-container'), function() {
      initIsotope = new Isotope(isotopeItem.querySelector('.isotope-container'), {
        itemSelector: '.isotope-item',
        layoutMode: layout,
        filter: filter,
        sortBy: sort
      });
    });

    isotopeItem.querySelectorAll('.isotope-filters li').forEach(function(filters) {
      filters.addEventListener('click', function() {
        isotopeItem.querySelector('.isotope-filters .filter-active').classList.remove('filter-active');
        this.classList.add('filter-active');
        initIsotope.arrange({
          filter: this.getAttribute('data-filter')
        });
        if (typeof aosInit === 'function') {
          aosInit();
        }
      }, false);
    });

  });

})();



// form valiudation

document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("contactForm");

    const fields = {
        firstName: {
            el: document.getElementById("firstName"),
            message: "First name is required"
        },
        lastName: {
            el: document.getElementById("lastName"),
            message: "Last name is required"
        },
        email: {
            el: document.getElementById("email"),
            message: "Enter a valid email address",
            regex: /^[^\s@]+@[^\s@]+\.[^\s@]+$/
        },
        contactNumber: {
            el: document.getElementById("contactNumber"),
            message: "Enter a valid 10-digit contact number",
            regex: /^\d{10}$/
        },
        appylIn: {
            el: document.getElementById("appylIn"),
            message: "This field is required"
        },
        message: {
            el: document.getElementById("message"),
            message: "Message is required"
        }
    };

    function showError(field, msg) {
        clearError(field);
        field.classList.add("is-invalid");

        const error = document.createElement("div");
        error.className = "error-text";
        error.innerText = msg;

        field.closest(".col-md-6, .col-md-12").appendChild(error);
    }

    function clearError(field) {
        field.classList.remove("is-invalid");

        const container = field.closest(".col-md-6, .col-md-12");
        const err = container.querySelector(".error-text");
        if (err) err.remove();
    }


    function validateForm() {
        let isValid = true;

        Object.keys(fields).forEach(key => {
            const field = fields[key];
            const value = field.el.value.trim();

            clearError(field.el);

            if (!value) {
                showError(field.el, field.message);
                isValid = false;
            }
            else if (field.regex && !field.regex.test(value)) {
                showError(field.el, field.message);
                isValid = false;
            }
        });

        return isValid;
    }

    //Only numbers in contact number
    fields.contactNumber.el.addEventListener("input", function () {
        this.value = this.value.replace(/\D/g, "").slice(0, 10);
    });

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        // Clear server error before validating
        const serverError = document.getElementById("serverError");
        serverError.style.display = "none";
        serverError.innerText = "";

        if (!validateForm()) return;

        const btn = form.querySelector("button[type='submit']");
        btn.disabled = true;
        btn.innerText = "Sending...";

        fetch(form.action, {
            method: "POST",
            body: new FormData(form),
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
            .then(res => res.json())
            .then(res => {
                if (res.success) {
                    window.location.href = res.redirectUrl;
                } else {
                    
                    serverError.innerText = res.message || "Something went wrong";
                    serverError.style.display = "block";

                    btn.disabled = false;
                    btn.innerText = "Send Message";
                }
            })
            .catch(() => {
                serverError.innerText = "Unexpected error occurred. Please try again.";
                serverError.style.display = "block";

                btn.disabled = false;
                btn.innerText = "Send Message";
            });
    });


});
