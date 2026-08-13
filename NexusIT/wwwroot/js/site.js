document.addEventListener("DOMContentLoaded", function () {

    const userMenuButton =
        document.getElementById("userMenuButton");

    const userDropdown =
        document.getElementById("userDropdown");

    if (userMenuButton && userDropdown) {

        userMenuButton.addEventListener("click", function (event) {

            event.stopPropagation();

            const isOpen =
                !userDropdown.hasAttribute("hidden");

            if (isOpen) {

                userDropdown.setAttribute("hidden", "");

                userMenuButton.setAttribute(
                    "aria-expanded",
                    "false"
                );

            } else {

                userDropdown.removeAttribute("hidden");

                userMenuButton.setAttribute(
                    "aria-expanded",
                    "true"
                );
            }

        });


        document.addEventListener("click", function (event) {

            if (!event.target.closest(".user-area")) {

                userDropdown.setAttribute(
                    "hidden",
                    ""
                );

                userMenuButton.setAttribute(
                    "aria-expanded",
                    "false"
                );
            }

        });

    }

});