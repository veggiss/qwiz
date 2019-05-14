axios.get('/api/wakeUp');

setInterval(() => {
    let lastActivity = window.localStorage.getItem("lastActivity");
    if (lastActivity == null || parseInt(lastActivity) < Date.now()) {
        axios.get('/api/wakeUp');
        window.localStorage.setItem("lastActivity", (Date.now() + 1000 * 60).toString());
    }
}, 1000 * 60);