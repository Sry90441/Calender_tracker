/**
 * Calendar Application - Main Controller
 * Manages calendar state, view switching, and API communication.
 * 
 * Architecture:
 * - CalendarState: Holds current view and date
 * - CalendarAPI: Handles all API calls
 * - CalendarRenderer: Renders different calendar views
 * - EventHandlers: Manages user interactions
 */

// ==========================================
// STATE MANAGEMENT
// ==========================================

const CalendarState = {
    currentViewDate: new Date(),
    currentViewType: 'monthly', // daily, weekly, monthly
    allEvents: [],
    allCategories: [],
    currentEditingEventId: null,

    setViewDate(date) {
        this.currentViewDate = new Date(date);
    },

    setViewType(type) {
        if (['daily', 'weekly', 'monthly'].includes(type)) {
            this.currentViewType = type;
        }
    },

    setEvents(events) {
        this.allEvents = events || [];
    },

    setCategories(categories) {
        this.allCategories = categories || [];
    }
};

// ==========================================
// API COMMUNICATION LAYER
// ==========================================

const CalendarAPI = {
    BASE_URL: '/api/calendar',

    async getEvents(startDate, endDate) {
        try {
            const startStr = startDate.toISOString().split('T')[0];
            const endStr = endDate.toISOString().split('T')[0];
            
            const response = await fetch(`${this.BASE_URL}?startDate=${startStr}&endDate=${endStr}`);
            
            if (!response.ok) {
                throw new Error('Failed to load events');
            }
            
            return await response.json();
        } catch (error) {
            console.error('API Error (getEvents):', error);
            throw error;
        }
    },

    async getEvent(eventId) {
        try {
            const response = await fetch(`${this.BASE_URL}/${eventId}`);
            
            if (!response.ok) {
                throw new Error('Event not found');
            }
            
            return await response.json();
        } catch (error) {
            console.error('API Error (getEvent):', error);
            throw error;
        }
    },

    async createEvent(eventData) {
        try {
            const response = await fetch(this.BASE_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(eventData)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Failed to create event');
            }

            return await response.json();
        } catch (error) {
            console.error('API Error (createEvent):', error);
            throw error;
        }
    },

    async updateEvent(eventId, eventData) {
        try {
            const response = await fetch(`${this.BASE_URL}/${eventId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(eventData)
            });

            if (!response.ok) {
                throw new Error('Failed to update event');
            }

            return await response.json();
        } catch (error) {
            console.error('API Error (updateEvent):', error);
            throw error;
        }
    },

    async deleteEvent(eventId) {
        try {
            const response = await fetch(`${this.BASE_URL}/${eventId}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                throw new Error('Failed to delete event');
            }

            return true;
        } catch (error) {
            console.error('API Error (deleteEvent):', error);
            throw error;
        }
    }
};

// ==========================================
// DATE UTILITIES
// ==========================================

const DateUtils = {
    getDateRange(viewType, date) {
        const start = new Date(date);
        let end = new Date(date);

        if (viewType === 'daily') {
            end.setDate(end.getDate() + 1);
        } else if (viewType === 'weekly') {
            const day = start.getDay();
            start.setDate(start.getDate() - day);
            end.setDate(start.getDate() + 7);
        } else { // monthly
            start.setDate(1);
            end = new Date(start.getFullYear(), start.getMonth() + 1, 1);
        }

        return { start, end };
    },

    getViewLabel(viewType, date) {
        const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };

        if (viewType === 'daily') {
            return date.toLocaleDateString('en-US', options);
        } else if (viewType === 'weekly') {
            const start = new Date(date);
            start.setDate(start.getDate() - start.getDay());
            const end = new Date(start);
            end.setDate(end.getDate() + 6);
            return `Week of ${start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${end.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}`;
        } else {
            return date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
        }
    },

    generateTimeSlots() {
        const slots = [];
        for (let i = 0; i < 24; i++) {
            slots.push(i.toString().padStart(2, '0') + ':00');
        }
        return slots;
    },

    toDateInputFormat(date) {
        return new Date(date).toISOString().slice(0, 16);
    }
};

// ==========================================
// CALENDAR RENDERER
// ==========================================

const CalendarRenderer = {
    render(events) {
        const viewType = CalendarState.currentViewType;
        let html = '';

        if (viewType === 'daily') {
            html = this.renderDailyView(events);
        } else if (viewType === 'weekly') {
            html = this.renderWeeklyView(events);
        } else {
            html = this.renderMonthlyView(events);
        }

        return html;
    },

    renderDailyView(events) {
        const date = CalendarState.currentViewDate;
        const dayEvents = events.filter(e => {
            const eDate = new Date(e.startDateTime).toDateString();
            return eDate === date.toDateString();
        });

        const timeSlots = DateUtils.generateTimeSlots();
        let html = '<div class="daily-view"><div class="daily-time-col">';

        // Time column
        timeSlots.forEach(time => {
            html += `<div class="daily-time-slot">${time}</div>`;
        });

        html += '</div><div class="daily-events-col" style="position: relative;">';

        // Events
        dayEvents.forEach(event => {
            const start = new Date(event.startDateTime);
            const hour = start.getHours();
            const minute = start.getMinutes();
            const topPercent = (hour * 60 + minute) / (24 * 60) * 100;
            const color = event.colorHex || '#007bff';

            html += `<div class="daily-event" data-event-id="${event.id}" style="top: ${topPercent}%; background: ${color};">
                ${this.escapeHtml(event.title)}
            </div>`;
        });

        html += '</div></div>';
        return html;
    },

    renderWeeklyView(events) {
        const start = new Date(CalendarState.currentViewDate);
        start.setDate(start.getDate() - start.getDay());
        const timeSlots = DateUtils.generateTimeSlots();

        let html = '<div class="weekly-grid">';

        // Time column header
        html += '<div style="background: #f8f9fa;"></div>';

        // Day headers
        for (let i = 0; i < 7; i++) {
            const day = new Date(start);
            day.setDate(day.getDate() + i);
            html += `<div class="weekly-time-header">${day.toLocaleDateString('en-US', { weekday: 'short', month: 'numeric', day: 'numeric' })}</div>`;
        }

        // Time slots and events
        timeSlots.forEach(time => {
            html += `<div class="weekly-time-slot hour-header">${time}</div>`;

            for (let i = 0; i < 7; i++) {
                const day = new Date(start);
                day.setDate(day.getDate() + i);
                const dayEvents = events.filter(e => {
                    const eDate = new Date(e.startDateTime).toDateString();
                    return eDate === day.toDateString();
                });

                let eventsHtml = dayEvents.map(event => {
                    const color = event.colorHex || '#007bff';
                    return `<div class="event-item" data-event-id="${event.id}" style="background: ${color};">
                        ${this.escapeHtml(event.title)}
                    </div>`;
                }).join('');

                html += `<div class="weekly-time-slot" data-slot-time="${time}">${eventsHtml}</div>`;
            }
        });

        html += '</div>';
        return html;
    },

    renderMonthlyView(events) {
        const year = CalendarState.currentViewDate.getFullYear();
        const month = CalendarState.currentViewDate.getMonth();
        const firstDay = new Date(year, month, 1);
        const lastDay = new Date(year, month + 1, 0);
        const startDate = new Date(firstDay);
        startDate.setDate(startDate.getDate() - firstDay.getDay());

        let html = '<table class="calendar-grid"><thead><tr>';
        const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
        
        days.forEach(day => html += `<th>${day}</th>`);
        html += '</tr></thead><tbody><tr>';

        let currentDate = new Date(startDate);
        for (let i = 0; i < 42; i++) {
            if (i > 0 && currentDate.getDay() === 0) {
                html += '</tr><tr>';
            }

            const isCurrentMonth = currentDate.getMonth() === month;
            const dateStr = currentDate.toISOString().split('T')[0];
            const dayEvents = events.filter(e => e.startDateTime.startsWith(dateStr));

            html += `<td data-date="${dateStr}">
                <div class="calendar-cell">
                    <span class="cell-date ${isCurrentMonth ? '' : 'other-month'}">${currentDate.getDate()}</span>
                    <div class="events-container">
                        ${dayEvents.map(event => {
                            const color = event.colorHex || '#007bff';
                            return `<div class="event-item ${event.isAllDay ? 'all-day' : ''}" 
                                data-event-id="${event.id}" 
                                style="background: ${color};" 
                                title="${this.escapeHtml(event.title)}">
                                ${this.escapeHtml(event.title)}
                            </div>`;
                        }).join('')}
                    </div>
                </div>
            </td>`;

            currentDate.setDate(currentDate.getDate() + 1);
        }

        html += '</tr></tbody></table>';
        return html;
    },

    escapeHtml(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, m => map[m]);
    }
};

// ==========================================
// EVENT HANDLERS & FORM MANAGEMENT
// ==========================================

const EventHandlers = {
    initialize() {
        this.setupViewSwitcher();
        this.setupNavigation();
        this.setupFormHandlers();
        this.setupRecurrenceOptions();
    },

    setupViewSwitcher() {
        document.querySelectorAll('.view-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const viewType = btn.dataset.view;
                CalendarState.setViewType(viewType);

                document.querySelectorAll('.view-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');

                CalendarController.loadAndRender();
            });
        });
    },

    setupNavigation() {
        document.getElementById('prevBtn').addEventListener('click', () => {
            const date = CalendarState.currentViewDate;

            if (CalendarState.currentViewType === 'daily') {
                date.setDate(date.getDate() - 1);
            } else if (CalendarState.currentViewType === 'weekly') {
                date.setDate(date.getDate() - 7);
            } else {
                date.setMonth(date.getMonth() - 1);
            }

            CalendarController.loadAndRender();
        });

        document.getElementById('nextBtn').addEventListener('click', () => {
            const date = CalendarState.currentViewDate;

            if (CalendarState.currentViewType === 'daily') {
                date.setDate(date.getDate() + 1);
            } else if (CalendarState.currentViewType === 'weekly') {
                date.setDate(date.getDate() + 7);
            } else {
                date.setMonth(date.getMonth() + 1);
            }

            CalendarController.loadAndRender();
        });
    },

    setupFormHandlers() {
        document.getElementById('addEventForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            await this.handleCreateEvent();
        });

        document.getElementById('editEventForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            await this.handleUpdateEvent();
        });

        document.getElementById('deleteEventBtn').addEventListener('click', async () => {
            await this.handleDeleteEvent();
        });
    },

    setupRecurrenceOptions() {
        document.getElementById('recurrencePattern').addEventListener('change', function() {
            document.getElementById('weeklyOptions').style.display = 
                this.value === 'Weekly' ? 'block' : 'none';
            document.getElementById('monthlyOptions').style.display = 
                this.value === 'Monthly' ? 'block' : 'none';
        });
    },

    async handleCreateEvent() {
        const form = document.getElementById('addEventForm');
        const formData = new FormData(form);

        let weeklyDays = 0;
        if (formData.get('recurrencePattern') === 'Weekly') {
            document.querySelectorAll('.weekly-day:checked').forEach(checkbox => {
                weeklyDays |= (1 << parseInt(checkbox.value));
            });
        }

        const eventData = {
            title: formData.get('title'),
            description: formData.get('description'),
            startDateTime: new Date(formData.get('startDateTime')).toISOString(),
            endDateTime: new Date(formData.get('endDateTime')).toISOString(),
            isAllDay: formData.get('isAllDay') === 'on',
            colorHex: formData.get('colorHex'),
            categoryId: formData.get('categoryId') ? parseInt(formData.get('categoryId')) : null,
            recurrencePattern: formData.get('recurrencePattern'),
            recurrenceEndDate: formData.get('recurrenceEndDate') ? new Date(formData.get('recurrenceEndDate')).toISOString() : null,
            weeklyRecurrenceDays: weeklyDays || null,
            monthlyRecurrenceDay: formData.get('monthlyRecurrenceDay') ? parseInt(formData.get('monthlyRecurrenceDay')) : null
        };

        try {
            await CalendarAPI.createEvent(eventData);
            form.reset();
            bootstrap.Modal.getInstance(document.getElementById('addEventModal')).hide();
            await CalendarController.loadAndRender();
            this.showNotification('Event created successfully!', 'success');
        } catch (error) {
            this.showNotification(error.message, 'danger');
        }
    },

    async handleUpdateEvent() {
        const eventId = document.getElementById('editEventId').value;
        const form = document.getElementById('editEventForm');
        const formData = new FormData(form);

        const eventData = {
            title: formData.get('title'),
            description: formData.get('description'),
            startDateTime: new Date(formData.get('startDateTime')).toISOString(),
            endDateTime: new Date(formData.get('endDateTime')).toISOString(),
            isAllDay: formData.get('isAllDay') === 'on',
            colorHex: formData.get('colorHex'),
            categoryId: formData.get('categoryId') ? parseInt(formData.get('categoryId')) : null,
            recurrencePattern: 'None'
        };

        try {
            await CalendarAPI.updateEvent(eventId, eventData);
            bootstrap.Modal.getInstance(document.getElementById('editEventModal')).hide();
            await CalendarController.loadAndRender();
            this.showNotification('Event updated successfully!', 'success');
        } catch (error) {
            this.showNotification(error.message, 'danger');
        }
    },

    async handleDeleteEvent() {
        const eventId = document.getElementById('editEventId').value;

        if (!confirm('Are you sure you want to delete this event?')) {
            return;
        }

        try {
            await CalendarAPI.deleteEvent(eventId);
            bootstrap.Modal.getInstance(document.getElementById('editEventModal')).hide();
            await CalendarController.loadAndRender();
            this.showNotification('Event deleted successfully!', 'success');
        } catch (error) {
            this.showNotification(error.message, 'danger');
        }
    },

    async openEditModal(eventId) {
        try {
            const event = await CalendarAPI.getEvent(eventId);
            CalendarState.currentEditingEventId = eventId;

            document.getElementById('editEventId').value = eventId;
            document.getElementById('editEventTitle').value = event.title;
            document.getElementById('editEventDescription').value = event.description || '';
            document.getElementById('editEventStart').value = DateUtils.toDateInputFormat(event.startDateTime);
            document.getElementById('editEventEnd').value = DateUtils.toDateInputFormat(event.endDateTime);
            document.getElementById('editEventAllDay').checked = event.isAllDay;
            document.getElementById('editEventColor').value = event.colorHex || '#007bff';
            document.getElementById('editEventCategory').value = event.categoryId || '';

            new bootstrap.Modal(document.getElementById('editEventModal')).show();
        } catch (error) {
            this.showNotification(error.message, 'danger');
        }
    },

    openQuickCreateModal(slotTime) {
        const date = new Date(CalendarState.currentViewDate);
        const [hour, minute] = slotTime.split(':').map(Number);
        date.setHours(hour, minute, 0);

        const endDate = new Date(date);
        endDate.setHours(endDate.getHours() + 1);

        document.getElementById('eventStart').value = DateUtils.toDateInputFormat(date);
        document.getElementById('eventEnd').value = DateUtils.toDateInputFormat(endDate);

        new bootstrap.Modal(document.getElementById('addEventModal')).show();
    },

    showNotification(message, type = 'info') {
        const alertId = 'alert-' + Date.now();
        const alertHtml = `<div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>`;

        document.body.insertAdjacentHTML('beforeend', alertHtml);
        setTimeout(() => {
            const el = document.getElementById(alertId);
            if (el) el.remove();
        }, 5000);
    }
};

// ==========================================
// MAIN CONTROLLER
// ==========================================

const CalendarController = {
    async initialize() {
        try {
            EventHandlers.initialize();
            await this.loadAndRender();
        } catch (error) {
            console.error('Initialization error:', error);
            this.showError('Failed to initialize calendar');
        }
    },

    async loadAndRender() {
        try {
            const { start, end } = DateUtils.getDateRange(
                CalendarState.currentViewType,
                CalendarState.currentViewDate
            );

            const events = await CalendarAPI.getEvents(start, end);
            CalendarState.setEvents(events);

            this.updateViewLabel();
            this.renderCalendar();
            this.attachEventListeners();
        } catch (error) {
            console.error('Load and render error:', error);
            this.showError(error.message);
        }
    },

    renderCalendar() {
        const html = CalendarRenderer.render(CalendarState.allEvents);
        document.getElementById('calendarView').innerHTML = html;
    },

    updateViewLabel() {
        const label = DateUtils.getViewLabel(
            CalendarState.currentViewType,
            CalendarState.currentViewDate
        );
        document.getElementById('viewLabel').textContent = label;
    },

    attachEventListeners() {
        // Event click handlers
        document.querySelectorAll('[data-event-id]').forEach(el => {
            el.addEventListener('click', (e) => {
                e.stopPropagation();
                const eventId = parseInt(el.dataset.eventId);
                EventHandlers.openEditModal(eventId);
            });
        });

        // Time slot click handlers
        document.querySelectorAll('[data-slot-time]').forEach(el => {
            el.addEventListener('click', (e) => {
                if (e.target === el) {
                    const slotTime = el.dataset.slotTime;
                    EventHandlers.openQuickCreateModal(slotTime);
                }
            });
        });
    },

    showError(message) {
        const errorHtml = `<div class="calendar-error">
            <strong>Error:</strong> ${message}
        </div>`;
        document.getElementById('calendarView').innerHTML = errorHtml;
    }
};

// ==========================================
// INITIALIZATION
// ==========================================

document.addEventListener('DOMContentLoaded', () => {
    CalendarController.initialize();
});
