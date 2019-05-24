// Add global utility methods here
let util = {
    apiUrl: function(path, obj) {
        let url = path + '?';
        let s = '';

        obj.forEach((item, key) => {
            if (item !== undefined || item !== undefined) url += `${s + key}=${item}`;
            s = '&';
        });
        
        if (global.debug) console.log(`Sending request to api: '${url}'`);

        return url;
    },
    openModal: function(text) {
        $("#globalModalMsg").text(text);
        $("#modalGlobal").modal()
    },
    logResponse: function(response) {
        if (response.status) {
            let s = response.status.toString()[0];
            let data = response.data ? response.data : 'no data';
            
            if (s === '5') {
                util.openModal("Server got an exception error, That ain't good..");
            } else if (s === '4') {
                if (data.title) {
                    util.openModal(data.title);
                } else {
                    if (data) util.openModal(data);
                } 
            } else {
                console.log(`Request responded with status ${response.status}, and data:`);
                console.log(data);
            }
        }
    }
};

// Global vars goes here
let global = {
    quizCardAmount: 12,
    membersCardAmount: 20,
    wakeUpTimer: 1000 * 60 * 5,
    isAuthenticated: false,
    currentUserName: null,
    debug: true
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
            for (let key in this) {
                if (this.hasOwnProperty(key)) {
                    callback.call(thisArg, this[key], key, this);
                }
            }
        }
    });
}

// Start intervall for wakeup request telling the server know the user is online
setInterval(() => {
    let lastActivity = window.localStorage.getItem("lastActivity");
    if (global.isAuthenticated && (lastActivity == null || parseInt(lastActivity) < Date.now())) {
        window.localStorage.setItem("lastActivity", (Date.now() + global.wakeUpTimer).toString());
        axios.put('/api/user/wakeUp');
    }
}, 1000 * 30);