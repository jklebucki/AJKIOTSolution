window.initializeDial = (dialId, dotNetRef) => {
    const dial = document.getElementById(dialId);
    const knob = dial.querySelector('.knob');
    let isDragging = false;

    function updateKnobPosition(angleDegrees) {
        // Przekształć kąt w radiany
        const angleRadians = (angleDegrees - 90) * (Math.PI / 180); // Ajust for initial position
        const rect = dial.getBoundingClientRect();
        // Oblicz promień, jako połowę szerokości dial, minus połowa szerokości knob dla środka
        const radius = rect.width / 2 - knob.offsetWidth / 2;
        // Oblicz nową pozycję X i Y dla knob
        const knobX = radius * Math.cos(angleRadians) + dial.offsetWidth / 2;
        const knobY = radius * Math.sin(angleRadians) + dial.offsetHeight / 2;

        // Zaktualizuj styl, aby przenieść knob do nowej pozycji
        knob.style.left = `${knobX}px`;
        knob.style.top = `${knobY}px`;
    }

    knob.addEventListener('mousedown', () => isDragging = true);

    document.addEventListener('mousemove', (event) => {
        if (isDragging) {
            const rect = dial.getBoundingClientRect();
            const centerX = rect.left + rect.width / 2;
            const centerY = rect.top + rect.height / 2;
            const angleRadians = Math.atan2(event.clientY - centerY, event.clientX - centerX);
            const angleDegrees = angleRadians * (180 / Math.PI) + 90; // Ajust for initial position
            dotNetRef.invokeMethodAsync('SetRotation', angleDegrees);
            updateKnobPosition(angleDegrees);
        }
    });

    document.addEventListener('mouseup', () => isDragging = false);
};