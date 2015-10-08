/*
	EE 4984:  Network Application Design
	Example of overlapped socket I/O using callback functions

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

#define RECV		0				// receive index
#define SEND		1				// send index

void errexit(const char *, ...);
void CALLBACK SendComplete(DWORD, DWORD, LPWSAOVERLAPPED, DWORD);
void CALLBACK RecvComplete(DWORD, DWORD, LPWSAOVERLAPPED, DWORD);



/*
	Global variables for use by main() and completion routines
*/
char   send_buf[BUFSIZE];			// buffer for sending to server
char   recv_buf[BUFSIZE];			// buffer for receiving from server
WSABUF send_wsabuf;					// WSABUF for sends
WSABUF recv_wsabuf;					// WSABUF for receives
int    send_cnt = 0;				// full count of bytes sent to server
int    recv_cnt = 0;				// full count of bytes received from server

WSAOVERLAPPED Overlapped[2];		// dummy overlapped structures for send and receive

SOCKET	sock;						// socket descriptor


void main(int argc, char *argv[])
{
	WSADATA	wsaData;				// dummy for returned data
	int		rc;						// return code

	DWORD  send_num;				// dummy number of bytes sent by last send
	DWORD  recv_num;				// dummy number of bytes received by last receive
	DWORD  recv_flags = 0;			// receive flags
	
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
	if((rc = WSAStartup(WSVERS, &wsaData)) != 0)
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
		Send first block of data to server using overlapped WSASend()
		call.

		int WSASend (
			SOCKET s,  
			LPWSABUF lpBuffers, 
			DWORD dwBufferCount,  
			LPDWORD lpNumberOfBytesSent,  
			DWORD dwFlags,  
			LPWSAOVERLAPPED lpOverlapped,  
			LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionROUTINE);

		Note that SendComplete() is the completion routine.
	*/
	rc = WSASend(sock, &send_wsabuf, 1, &send_num, 0, &Overlapped[SEND], (LPWSAOVERLAPPED_COMPLETION_ROUTINE) SendComplete);
	printf("Sending %d bytes:  return=%d, error=%d\n", send_wsabuf.len, rc, WSAGetLastError());
	if(rc == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		errexit("Send error: %d\n", WSAGetLastError());


	/*
		Request first block of data from server using overlapped WSARecv()
		call.

		int WSARecv (
			SOCKET s, 
			LPWSABUF lpBuffers,  
			DWORD dwBufferCount,  
			LPDWORD lpNumberOfBytesRecvd,  
			LPDWORD lpFlags,  
			LPWSAOVERLAPPED lpOverlapped,  
			LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionROUTINE);

			Note that RecvComplete() is the completion routine.
 	*/
	rc = WSARecv(sock, &recv_wsabuf, 1, &recv_num, &recv_flags, &Overlapped[RECV], (LPWSAOVERLAPPED_COMPLETION_ROUTINE) RecvComplete);
	printf("Receiving up to %d bytes:  return=%d, error=%d\n", recv_wsabuf.len, rc, WSAGetLastError());
	if(rc == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
		errexit("Receive error: %d\n", WSAGetLastError());


	/*
		At this point, a WSASend() and WSARecv() call are pending.  As each
		call completes, SendComplete() or RecvComplete() will be called, but
		only if the main (and only) thread is in an "alertable" state.  This
		state is entered using the SleepEx() call.

		DWORD SleepEx(
			DWORD dwMilliseconds,
			BOOL bAlertable);

		SleepEx() returns if one or more I/O completion callback functions
		execute.  However, there may be more, so the client continues to
		go back to sleep until all data is received.
	*/
	do {
		SleepEx(INFINITE, TRUE);
	} while(recv_cnt < SIZEDATA);

	
	/*
		Before quiting, close socket and terminate WinSock with usual
		closesocket() and WSACleanup() calls.
	*/
	closesocket(sock);
	WSACleanup();	
	
} // main()



/*
	Send completion routine

	This routine is called when the overlapped send completes.  The
	cbTransferred parameter specifies the number of bytes actually
	sent.

	The routine does another send if there is more data to send.
*/
void CALLBACK SendComplete(DWORD dwError, DWORD cbTransferred, LPWSAOVERLAPPED lpOverlapped, DWORD dwFlags)
{
	int rc;							// return code
	DWORD  send_num;				// dummy number of bytes sent

	if(dwError != 0)
		errexit("Send completion error: %d\n", WSAGetLastError());

	send_cnt += cbTransferred;
	printf("Send completed: %d (total %d)\n", cbTransferred, send_cnt);

	if(send_cnt < SIZEDATA)	{
		send_wsabuf.len = MIN(sizeof(send_buf), (SIZEDATA - send_cnt));
		rc = WSASend(sock, &send_wsabuf, 1, &send_num, 0, &Overlapped[SEND], (LPWSAOVERLAPPED_COMPLETION_ROUTINE) SendComplete);
		if(rc == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
			errexit("Send call error: %d\n", WSAGetLastError());
		printf("Sending %d bytes:  return=%d, error=%d\n", send_wsabuf.len, rc, WSAGetLastError());
	}

} // SendComplete()



/*
	Receive completion routine

	This routine is called when the overlapped receive completes.  The
	cbTransferred parameter specifies the number of bytes actually
	received.

	The routine does another receive if not all data is already received.
*/
void CALLBACK RecvComplete(DWORD dwError, DWORD cbTransferred, LPWSAOVERLAPPED lpOverlapped, DWORD dwFlags)
{
	int rc;							// return code
	DWORD  recv_num;				// dummy number of bytes received
	DWORD  recv_flags = 0;			// receive flags

	if(dwError != 0)
		errexit("Receive completion error: %d\n", WSAGetLastError());

	recv_cnt += cbTransferred;
	printf("Receive completed: %d (total %d)\n", cbTransferred, recv_cnt);
			
	if(recv_cnt < SIZEDATA)	{
		rc = WSARecv(sock, &recv_wsabuf, 1, &recv_num, &recv_flags, &Overlapped[RECV],(LPWSAOVERLAPPED_COMPLETION_ROUTINE) RecvComplete);
		if(rc == SOCKET_ERROR && WSAGetLastError() != WSA_IO_PENDING)
			errexit("Receive call error: %d\n", WSAGetLastError());
		printf("Receiving up to %d bytes:  return=%d, error=%d\n", recv_wsabuf.len, rc, WSAGetLastError());
	}

} // RecvComplete()



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
