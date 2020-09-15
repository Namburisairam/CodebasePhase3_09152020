importScripts('https://www.gstatic.com/firebasejs/4.2.0/firebase.js');
  // Initialize Firebase
  var config = {
      apiKey: "AIzaSyCBVQoAGTG11qNhAOZATtflin_e6z1lTX4",
      projectId: "eventapp-768e5",
      messagingSenderId: "784979102180"
  };
  firebase.initializeApp(config);

const messaging = firebase.messaging();

messaging.setBackgroundMessageHandler(function(payload) {
  console.log('[firebase-messaging-sw.js] Received background message ', payload);
  // Customize notification here
  const notificationTitle = 'Message Alert';
  const notificationOptions = {
      body: payload.data.twi_body,
    icon: '/firebase-logo.png'
  };

  return self.registration.showNotification(notificationTitle,
      notificationOptions);
});