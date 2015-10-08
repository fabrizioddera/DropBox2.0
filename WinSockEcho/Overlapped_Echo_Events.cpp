/*
	EE 4984:  Network Application Design
	Example of overlapped socket I/O using events

	Function is a standard echo client
	Use:  program [host [service]]

	Note that this example requires WinSock 2.0 or later.
	Include Ws2_32.lib in project.

	SFM (11/17/97)
*/


#include <stdio.h>
#include <string.h>
#include <stdarg.h>
#include <winsock2.h>

#define	MIN(x,y)	((x)>(y) ? (y) : (x))

#define WSVERS      MAKEWORD(2,0)
#define BUFSIZE     1024
#define SIZEDATA    4500

#define RECV		0				// receive index (must be 0)
#define SEND		1				// send index


void errexit(const char *, ...);


void main(int argc, char *argv[])
{
	char   send_buf[BUFSIZE];		// buffer for sending to server
	char   recv_buf[BUFSIZE];		// buffer for receiving from server
	WSABUF send_wsabuf;				// WSABUF for sends
	WSABUF recv_wsabuf;				// WSABUF for receives
	int    send_cnt = 0;			// full count of bytes sent to server
	int    recv_cnt = 0;			// full count of bytes received from server
	DWORD  send_num;				// number of bytes sent by last send
	DWORD  recv_num;				// number of bytes received by last receive
	int    send_rc;					// send return code
	int    recv_rc;					// receive return code
	DWORD  recv_flags = 0;			// receive flags
	DWORD  dummy_flags;				// dummy for returned flags

	DWORD  num_events = 2;			// number of events to check
	DWORD  sig_event;				// signalled event number
	WSAEVENT Event[2];				// events for send and receive
	WSAOVERLAPPED Overlapped[2];	// overlapped structures for send and receive

	WSADATA	wsaData;				// dummy for returned data
	int		rc;						// return code
	SOCKET	sock;					// socket descriptor
	
	struct hostent	*phe;			// pointer to host information entry
	struct servent	*pse;			// pointer to service information entry
	struct protoent *ppe;			// pointer to protocol information entry
	struct sockaddr_in sin;			// an Internet endpoint address

	char * host = "localhost";		// default host
	char * service = "echo";		// default service


	/*
		Get host and service (or port number) if specified in command line.
	*/
	switch (argc) {
	case 1:
		break;
	case 3:
		service = argv[2];
		/* fall through */
	case 2:
		host = argv[1];
		break;
	default:
		fprintf(stderr, "Usage:  %s [host [port]]\n", argv[0]);
		exit(1);
	}


	/*
		Fill up send buffer with fixed data (all ASCII A)
	*/
	memset(send_buf, 'A', sizeof(send_buf));


	/*
		Start up WinSock with usual call to WSAStartup().
	*/
	if((rc = WSAStartup(MAKEWORD(1,1), &wsaData)) != 0)
		errexit("Startup failed: %d\n", rc);


	/*
		Setup server host address, port information, and protocol type
		using the standard getXbyY() calls.
    */
	memset(&sin, 0, sizeof(sin));
	sin.sin_family = AF_INET;

    if (pse = getservbyname(service, "tcp")) sin.sin_port = pse->s_port;
	else if ((sin.sin_port = htons((u_short)atoi(service))) == 0)
		errexit("Can't get \"%s\" service entry\n", service);

	if (phe = gethostbyname(host)) memcpy(&sin.sin_addr, phe->h_addr, phe->h_length);
	else if ((sin.sin_addr.s_addr = inet_addr(host)) == INADDR_NONE)
		errexit("Can't get \"%s\" host entry\n", host);

    if ((ppe = getprotobyname("tcp")) == 0)
		errexit("Can't get \"tcp\" protocol entry\n");


	/*
		Create a socket using usual call to socket().  Note that socket(),
		by default, sets the overlapped attribute for the socket.  If we
		used WSASocket(), we would need to explicitly set the
		WSA_FLAG_OVERLAPPED flag.
	*/
	if((sock = socket(AF_INET, SOCK_STREAM, ppe->p_proto)) == INVALID_SOCKET)
		errexit("Can't create socket: %d\n", WSAGetLastError());


    /*
		Connect the socket using the usual call to connect().
	*/
	if (connect(sock, (struct sockaddr *)&sin, sizeof(sin)) == SOCKET_ERROR)
		errexit("Can't connect to host %s, service %s: %d\n", host, service, GetLastError());


	/*
		Create send and receive events using WSACreateEvent() and set
		hEvent field of corresponding WSAOVERLAPPED structures.

		WSAEVENT WSACreateEvent(void); 
	*/
	if((Event[SEND] = WSACreateEvent()) == WSA_INVALID_EVENT)
		errexit("Can't create send event: %d\n", WSAGetLastError());
	Overlapped[SEND].hEvent = Event[SEND];

	if((Event[RECV] = WSACreateEvent()) == WSA_INVALID_EVENT)
		errexit("Can't create receive event: %d\n", WSAGetLastError());
	Overlapped[RECV].hEvent = Event[RECV];


	/*
		Set up WSABUF buffer for send and receive calls.

		typedef struct _WSABUF {
			u_long      len;
			char FAR *  buf;
		} WSABUF, FAR * LPWSABUF;
	*/
	send_wsabuf.len = MIN(sizeof(send_buf), (SIZEDATA - send_cnt));
	send_wsabuf.buf = send_buf;

	recv_wsabuf.len = sizeof(recv_buf);
	recv_wsabuf.buf = recv_buf;
	
	  
	/*
		Send data to server using overlapped WSASend() call.

		int WSASend (
			SOCKET s,  
			LPWSABUF lpBuffers, 
			DWORD dwBufferCount,  
			LPDWORD lpNumberOfBytesSent,  
			DWORD dwFlags,  
			LPWSAOVERLAPPED lpOverlapped,  
			LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionROUTINE); 
	*/
	send_rc = WSASend(sock, &send_wsabuf, 1, &send_num, 0, &Overlapped[SEND], NULL);
	printf("Sending %d bytes:  return=%d, error=%d\n", send_wsabuf.len, send_rc, WSAGetLastError());
	if(send_rc == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		errexit("Send error: %d\n", WSAGetLastError());


	/*
		Receive data from server using overlapped WSARecv() call.

		int WSARecv (
			SOCKET s, 
			LPWSABUF lpBuffers,  
			DWORD dwBufferCount,  
			LPDWORD lpNumberOfBytesRecvd,  
			LPDWORD lpFlags,  
			LPWSAOVERLAPPED lpOverlapped,  
			LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionROUTINE);
 	*/
	recv_rc = WSARecv(sock, &recv_wsabuf, 1, &recv_num, &recv_flags, &Overlapped[RECV], NULL);
	printf("Receiving up to %d bytes:  return=%d, error=%d\n", recv_wsabuf.len, recv_rc, WSAGetLastError());
	if(recv_rc == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		errexit("Receive error: %d\n", WSAGetLastError());


	/*
		At this point, a WSASend() and WSARecv() call are pending.  The
		program now loops until all data is both sent and received.
		It waits until an event associated with send or receive, as set
		in the WSAOVERLAPPED structure passed to WSASend() and WSARecv(),
		is signalled before it attempts to do another operation.
	*/
	while(recv_cnt < SIZEDATA) {

		/*
			Wait for either Event[SEND] or Event[RECV] to be signalled
			using WSAWaitForMultipleEvnets().

			DWORD WSAWaitForMultipleEvents(
				DWORD cEvents,  
				const WSAEVENT FAR *lphEvents,  
				BOOL fWaitAll,  
				DWORD dwTimeOUT,  
				BOOL fAlertable);

			Note that the event count (num_events) is two while the
			client is both sending and receiving and then is reduced
			to one when only receiving.
		*/
		
		if((sig_event = WSAWaitForMultipleEvents(num_events, Event, FALSE, WSA_INFINITE, FALSE)) == WSA_WAIT_FAILED)
			errexit("Wait for event failed: %d\n", WSAGetLastError());

		if(sig_event == SEND) {


			/*
				The send completed.  Use WSAGetOverlappedResult() to
				determine number of bytes actually sent.

				BOOL WSAGetOverlappedResult (
					SOCKET s,  
					LPWSAOVERLAPPED lpOverlapped,  
					LPDWORD lpcbTransfer,  
					BOOL fWait,  
					LPDWORD lpdwFlags); 
			*/
			if(!WSAGetOverlappedResult(sock, &Overlapped[SEND], &send_num, TRUE, &dummy_flags))
				errexit("Can't get results for send: %d\n", WSAGetLastError());
						
			
			/*
				Update count and send more data if not done.  If not done,
				first reset the send event with WSAResetEvent() and then
				do the send with WSASend().

				BOOL WSAResetEvent(
					WSAEVENT hEvent); 
 
				If done, stop looking for send event by decrementing the
				number of events checked (num_events).  Note that
				Event[RECV]	will always be checked.
			*/
			send_cnt += send_num;
			printf("Send completed: %d (total %d)\n", send_num, send_cnt);
			send_wsabuf.len = MIN(sizeof(send_buf), (SIZEDATA - send_cnt));

			if(send_cnt < SIZEDATA)	{
				if(!WSAResetEvent(Event[SEND]))
					errexit("Can't reset send event: %d\n", WSAGetLastError());
				send_rc = WSASend(sock, &send_wsabuf, 1, &send_num, 0, &Overlapped[SEND], NULL);
				if(send_rc == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
					errexit("Send error: %d\n", WSAGetLastError());
				printf("Sending %d bytes:  return=%d, error=%d\n", send_wsabuf.len, send_rc, WSAGetLastError());
			}
			else --num_events;
		}

		else if(sig_event == RECV) {


			/*
				The receive completed.  Use WSAGetOverlappedResult() to
				determine the number of bytes actually received.
			*/
			if(!WSAGetOverlappedResult(sock, &Overlapped[RECV], &recv_num, TRUE, &dummy_flags))
				errexit("Can't get results for receive: %d\n", WSAGetLastError());
			
						
			/*
				Update count and receive more if not done.  If not done,
				first reset the receive event with WSAResetEvent() and
				then do the send with WSARecv().
			*/
			recv_cnt += recv_num;
			printf("Receive completed: %d (total %d)\n", recv_num, recv_cnt);
			
			if(recv_cnt < SIZEDATA)	{
				if(!WSAResetEvent(Event[RECV]))
					errexit("Can't reset receive event: %d\n", WSAGetLastError());
				recv_rc = WSARecv(sock, &recv_wsabuf, 1, &recv_num, &recv_flags, &Overlapped[RECV], NULL);
				if(recv_rc == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
					errexit("Receive error: %d\n", WSAGetLastError());
				printf("Receiving up to %d bytes:  return=%d, error=%d\n", recv_wsabuf.len, recv_rc, WSAGetLastError());
			}
		}

		else errexit("Invalid event returned (should not occur): %d\n", sig_event);

	}


	/*
		Before quitting, close socket and terminate WinSock with usual
		closesocket() and WSACleanup() calls.
	*/
	closesocket(sock);
	WSACleanup();	
	
} // main()



/*
	Error exit routine as in Comer and Stevens, Vol. III (Winsock edition)
*/
void errexit(const char *format, ...)
{
	va_list	args;

	va_start(args, format);
	vfprintf(stderr, format, args);
	va_end(args);
	WSACleanup();
	exit(1);

} // errexit()
