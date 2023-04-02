// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

setTimeout(func, 3000);

function func() {
    for (let i = 0; i < document.getElementsByClassName("alert").length; i++) {
        let a = document.getElementsByClassName("alert")[i];
        document.querySelector("main").removeChild(a);
    }
}