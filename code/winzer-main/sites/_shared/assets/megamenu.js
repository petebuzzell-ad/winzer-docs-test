document.querySelectorAll('.main-menu-item').forEach(item => {
    item.addEventListener('mouseenter', () => {
        document.querySelectorAll('.main-menu-item').forEach(i => {
            i.classList.remove('active');
        });
        
        item.classList.add('active');
    });
});
