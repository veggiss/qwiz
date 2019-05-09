// Add global utility methods here
let util = {
    apiUrl: function (path, obj) {
        let url = path + '?';
        let s = '';

        obj.forEach((item, key) => {
            if (item !== undefined || item !== undefined) url += `${s + key}=${item}`;
            s = '&';
        });

        return url;
    },
    openModal: function (text) {
        $("#globalModalMsg").text(text);
        $("#modalGlobal").modal()
    }
    
};

// Global vars goes here
let global = {
    quizCardAmount: 6
};

// Adds paginate component to vue
Vue.component('paginate', VuejsPaginate);

// Adds .forEach method on object types
if (!Object.prototype.forEach) {
    Object.defineProperty(Object.prototype, 'forEach', {
        value: function (callback, thisArg) {
            if (this == null) {
                throw new TypeError('Not an object');
            }
            thisArg = thisArg || window;
            for (var key in this) {
                if (this.hasOwnProperty(key)) {
                    callback.call(thisArg, this[key], key, this);
                }
            }
        }
    });
}

// Start intervall for wakeup request telling the server that the user is online
setInterval(() => {
    let lastActivity = window.localStorage.getItem("lastActivity");
    if (lastActivity == null || parseInt(lastActivity) < Date.now()) {
        axios.get('/api/wakeUp');
        window.localStorage.setItem("lastActivity", (Date.now() + 1000 * 60).toString());
    }
}, 1000 * 60);