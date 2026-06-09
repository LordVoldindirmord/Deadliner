(function () {
    const MONTHS = [
        'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
        'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'
    ];
    const WEEKDAYS = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];

    let openPicker = null;

    function parseDate(value) {
        if (!value) return null;
        const [year, month, day] = value.split('-').map(Number);
        if (!year || !month || !day) return null;
        return new Date(year, month - 1, day);
    }

    function formatIso(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    function formatDisplay(date) {
        const d = String(date.getDate()).padStart(2, '0');
        const m = String(date.getMonth() + 1).padStart(2, '0');
        return `${d}.${m}.${date.getFullYear()}`;
    }

    function isSameDay(a, b) {
        return a.getFullYear() === b.getFullYear()
            && a.getMonth() === b.getMonth()
            && a.getDate() === b.getDate();
    }

    function closePicker(picker) {
        picker.classList.remove('date-picker--open');
        picker.querySelector('.date-picker__popup').hidden = true;
        if (openPicker === picker) openPicker = null;
    }

    function closeAllPickers() {
        if (openPicker) closePicker(openPicker);
    }

    function renderCalendar(picker, viewDate, selectedDate) {
        const grid = picker.querySelector('.date-picker__grid');
        const title = picker.querySelector('.date-picker__month');
        const year = viewDate.getFullYear();
        const month = viewDate.getMonth();

        title.textContent = `${MONTHS[month]} ${year}`;
        grid.innerHTML = '';

        const firstDay = new Date(year, month, 1);
        const startOffset = (firstDay.getDay() + 6) % 7;
        const daysInMonth = new Date(year, month + 1, 0).getDate();
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        for (let i = 0; i < startOffset; i++) {
            const empty = document.createElement('span');
            empty.className = 'date-picker__day date-picker__day--empty';
            grid.appendChild(empty);
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const date = new Date(year, month, day);
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'date-picker__day';
            button.textContent = String(day);
            button.dataset.date = formatIso(date);

            if (isSameDay(date, today)) button.classList.add('date-picker__day--today');
            if (selectedDate && isSameDay(date, selectedDate)) button.classList.add('date-picker__day--selected');

            button.addEventListener('click', (e) => {
                e.stopPropagation();
                setValue(picker, date);
                renderCalendar(picker, viewDate, date);
            });

            grid.appendChild(button);
        }
    }

    function setValue(picker, date) {
        const hidden = picker.querySelector('input[type="hidden"]');
        const trigger = picker.querySelector('.date-picker__trigger');
        hidden.value = formatIso(date);
        trigger.textContent = formatDisplay(date);
        picker.dataset.viewYear = String(date.getFullYear());
        picker.dataset.viewMonth = String(date.getMonth());
    }

    function openPickerPanel(picker) {
        if (openPicker && openPicker !== picker) closePicker(openPicker);

        const hidden = picker.querySelector('input[type="hidden"]');
        const selected = parseDate(hidden.value) || new Date();
        const viewYear = Number(picker.dataset.viewYear ?? selected.getFullYear());
        const viewMonth = Number(picker.dataset.viewMonth ?? selected.getMonth());
        const viewDate = new Date(viewYear, viewMonth, 1);

        renderCalendar(picker, viewDate, selected);
        picker.querySelector('.date-picker__popup').hidden = false;
        picker.classList.add('date-picker--open');
        openPicker = picker;
    }

    function shiftMonth(picker, delta) {
        const hidden = picker.querySelector('input[type="hidden"]');
        const selected = parseDate(hidden.value);
        let viewYear = Number(picker.dataset.viewYear);
        let viewMonth = Number(picker.dataset.viewMonth);

        if (Number.isNaN(viewYear) || Number.isNaN(viewMonth)) {
            const base = selected || new Date();
            viewYear = base.getFullYear();
            viewMonth = base.getMonth();
        }

        const viewDate = new Date(viewYear, viewMonth + delta, 1);
        picker.dataset.viewYear = String(viewDate.getFullYear());
        picker.dataset.viewMonth = String(viewDate.getMonth());
        renderCalendar(picker, viewDate, selected);
    }

    function initPicker(picker) {
        const hidden = picker.querySelector('input[type="hidden"]');
        const initial = parseDate(hidden.value);
        if (initial) setValue(picker, initial);

        picker.querySelector('.date-picker__trigger').addEventListener('click', (e) => {
            e.stopPropagation();
            if (picker.classList.contains('date-picker--open')) {
                closePicker(picker);
            } else {
                openPickerPanel(picker);
            }
        });

        picker.querySelector('.date-picker__prev').addEventListener('click', (e) => {
            e.stopPropagation();
            shiftMonth(picker, -1);
        });

        picker.querySelector('.date-picker__next').addEventListener('click', (e) => {
            e.stopPropagation();
            shiftMonth(picker, 1);
        });

        picker.querySelector('.date-picker__popup').addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }

    document.addEventListener('click', () => closeAllPickers());

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeAllPickers();
    });

    document.querySelectorAll('[data-date-picker]').forEach(initPicker);
})();
