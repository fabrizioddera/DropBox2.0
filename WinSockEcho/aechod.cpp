/*
	EE 4984:  Network Application Design
	Example of asynchronous socket I/O using events

	Function is a standard TCP echo server
	Use:  program [service]

	SFM (10/20/98)
*/


#include <stdio.h>
#include <string.h>
#include <stdarg.h>
#include <winsock2.h>


// Constants
#define WSVERS      MAKEWORD(2,2)
#define BUFSIZE     1024
#define MAXSOCKS	11


// Macro to test if bit marked by b is set in v
#define BITSET(v,b)	(b & v)


// Function prototypes
u_short	MasterSocket(char *);
int		AcceptConnection(int, int);
int		CloseConnection(int, int);
int		RecvData(int, int);
int		SendData(int, int);
void	errexit(const char *, ...);
void	DisplayEvents(long, int);


// State definitions for client connections
#define ST_READING 1			// awaiting a read
#define ST_WRITING 2			// awaiting a write, client still sending
#define ST_CLOSING 3			// awaiting a write, client closed connection


// Per socket information
struct info {
	SOCKET sock;				// associated socket (master socket too)
	int	state;					// connection state
	char buf[BUFSIZE];			// data buffer
	char * bufptr;				// buffer pointer
	int buflen;					// bytes in buffer
};

struct info * Socks[MAXSOCKS];	// array of info structure pointers
WSAEVENT Events[MAXSOCKS];		// array of event handles
int NumSocks;					// number of active event handles and sockets



////////////////////
// Main procedure

void main(int argc, char *argv[])
{
	char *				service = "echo";	// default service
	u_short				port;				// port
	int					SigEvent;			// signaled event index
	WSANETWORKEVENTS	NetEvents;			// structure for network event information


	// Get service (or port number) if specified in command line.
	switch (argc) {
	case 1:
		break;
	case 2:
		service = argv[2];
		break;
	default:
		fprintf(stderr, "Arguments:  [service | port]\n");
		exit(1);
	}

	
	// Create master socket and set for asynchronous event notification.
	port = MasterSocket(service);
	printf("Asynchronous TCP echo server listening on port %d\n", port);
	
	
	// Infinite server loop follows.
	while (1) {

		// Wait for any event to be signalled.
		// For the master socket, this can only be triggered by ACCEPT.
		// For a slave socket, this can be triggered by READ, WRITE or CLOSE.
		// This call will block as long as needed for signal to occur.
		if((SigEvent = WSAWaitForMultipleEvents(NumSocks, Events, FALSE, WSA_INFINITE, TRUE)) == WSA_WAIT_FAILED)
			errexit("Wait for event failed: %d\n", WSAGetLastError());

		// One socket, indicated by SigEvent, has been signalled for one of
		// the event types.  The following call determines the event type or types.
		// See WSAEnumNetworkEvents() document for a description of the NetEvents
		// structure.
		if(WSAEnumNetworkEvents(Socks[SigEvent]->sock, Events[SigEvent], &NetEvents) == SOCKET_ERROR)
			errexit("Enumeration of network events failed: %d\n", WSAGetLastError());

		// The following call is for illustrative purposes only.  It displays
		// the event types that are ready.
		DisplayEvents(NetEvents.lNetworkEvents, SigEvent);
  		
		// Check for accepted connection.  This will only occur for master socket.
		if (BITSET(NetEvents.lNetworkEvents,FD_ACCEPT))
			AcceptConnection(SigEvent, NetEvents.iErrorCode[FD_ACCEPT_BIT]);

		// Check for data ready to be received.  This will only occur for a
		// slave socket.  Note that this event type will be signalled when data
		// is available to be received.  It will be retriggered anytime new
		// data arrives after a receive is executed or if the receive does not
		// read all available data.
		if(BITSET(NetEvents.lNetworkEvents,FD_READ))
			RecvData(SigEvent, NetEvents.iErrorCode[FD_READ_BIT]);

		// Check for data ready to be sent.  This will occur only for a slave
		// socket.  Note that this event type is signalled after a connection
		// is first accepted and, after that, only if a send blocks.  So,
		// data is sent in the RecvData() routine and the ST_WRITING state is
		// entered only if a send blocks.
		if(BITSET(NetEvents.lNetworkEvents,FD_WRITE))
			SendData(SigEvent, NetEvents.iErrorCode[FD_READ_BIT]);

		// Check for client closing connection.  This will only occur for a
		// slave socket.  Note that this event type occurs if the client
		// closes or shuts down their socket.  CloseConnection() will close
		// the connection from the server side only if not in the ST_WRITING
		// state (i.e., only if there is no more data to write to the client).
		if(BITSET(NetEvents.lNetworkEvents,FD_CLOSE))
			CloseConnection(SigEvent, NetEvents.iErrorCode[FD_READ_BIT]);
	}

} // main()



////////////////////////////////////////////////////////
// Create master socket for event-driven notification

u_short MasterSocket(char * service)
{
	WSADATA	wsadata;			// sockets startup data
	struct servent * pse;		// pointer to service information entry
	struct sockaddr_in sin;		// an Internet endpoint address
	SOCKET temp_sock;		// temporary socket value

	// Start WinSock (standard call).
	if (WSAStartup(WSVERS, &wsadata) != 0)
		errexit("WSAStartup failed\n");

    // Allocate a socket (standard call).
	if ((temp_sock = socket(PF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET)
		errexit("Error creating socket: %d\n", GetLastError());

	// Create event object to be set by asychronous events.
	// Note that master socket is entry 0 in Events[] and Socks[] arrays.
	if ((Events[0] = WSACreateEvent()) == WSA_INVALID_EVENT)
		errexit("Error creating event object: %d\n", WSAGetLastError());
	
	// Make socket non-blocking with asynchronous event notification for ACCEPT.
	// This call associates the event handle created above with the master socket.
	// Master socket will be "signaled" when call to accept() will not block.
	if (WSAEventSelect(temp_sock, Events[0], FD_ACCEPT) == SOCKET_ERROR)
		errexit("Error initiating asynchronous event notification: %d\n", WSAGetLastError());

	// Initialize info structure for master socket.
	// Again, note that master socket is entry 0 in Socks[] array.
	if ((Socks[0] = (struct info *) malloc(sizeof(struct info))) == NULL)
		errexit("Memory alloation error\n");

	Socks[0]->sock = temp_sock;
	NumSocks = 1;

	// Create address for master socket (standard code).
	memset(&sin, 0, sizeof(sin));
	sin.sin_family = AF_INET;
	sin.sin_addr.s_addr = INADDR_ANY;

	if (pse = getservbyname(service, "tcp"))
		sin.sin_port = (u_short) pse->s_port;
	else if ((sin.sin_port = htons((u_short)atoi(service))) == 0)
		errexit("Can't get \"%s\" service entry\n", service);

	// Bind address to the socket (standard call).
	if (bind(temp_sock, (struct sockaddr *)&sin, sizeof(sin)) == SOCKET_ERROR)
		errexit("can't bind to %s port: %d\n", service, WSAGetLastError());
	
	// Make socket passive (standard call).
	if(listen(temp_sock, 5) == SOCKET_ERROR)
		errexit("Error listening on %s port: %d\n", service, WSAGetLastError());

	return(ntohs(sin.sin_port));

} // end MasterSocket()



////////////////////////////////////////////////////////////
// Accept a connection (only in phantom "awaiting" state)

int AcceptConnection(int EventNum, int EventError)
{
	struct sockaddr_in new_sin;	// client address for accept
	int addrlen;				// length of client address
	SOCKET temp_sock;			// temporary socket descriptor


	// Check for error in event notification.
	if (EventError != 0) {
		printf("Accept error:  %d\n", EventError);
		return -1;
	}

	// Accept the connection (standard call).
	addrlen = sizeof(new_sin);
	if ((temp_sock = accept(Socks[EventNum]->sock, (struct sockaddr *) &new_sin, &addrlen)) == INVALID_SOCKET) {
		printf("Accept event error: %d\n", WSAGetLastError());
		return -1;
	}
	
	// Check if new connection exceeds server capacity.  The limit is
	// due to Events[] and Socks[] being fixed-size arrays.  An excess
	// client connection is simply closed. 
	if (NumSocks >= MAXSOCKS) {
		printf("Too many connections - aborting new connection\n");
		closesocket(temp_sock);
		return -1;
	}

	// The client connection can be serviced.  Information for this connection
	// is saved in an info structure pointed to by an entry in the Socks[]
	// array.  Note that the connection starts in the ST_READING state since
	// data is first to be read from an ECHO client.
	if ((Socks[NumSocks] = (struct info *) malloc(sizeof(struct info))) == NULL) {
		printf("Memory alloation error\n");
		closesocket(temp_sock);
		return -1;
	}
	Socks[NumSocks]->sock = temp_sock;
	Socks[NumSocks]->state = ST_READING;


	// Create event object to be set by asychronous events for new socket.
	// Note that the event handle is in the same location in Events[] as
	// connectin information is in Socks[].
	if ((Events[NumSocks] = WSACreateEvent()) == WSA_INVALID_EVENT) {
		printf("Error creating event object: %d\n", WSAGetLastError());
		closesocket(temp_sock);
		return -1;
	}

	// Make socket non-blocking with asynchronous event notification.  This
	// call associates the event handle just created with the slave socket
	// and tells WinSock to notify for READ and CLOSE events.
	if (WSAEventSelect(Socks[NumSocks]->sock, Events[NumSocks], FD_READ | FD_CLOSE) == SOCKET_ERROR) {
		printf("Error initiating asynchronous event notification: %d\n", WSAGetLastError());
		closesocket(temp_sock);
		return -1;
	}

	printf("Status: new connection accepted (%d)\n", NumSocks);
	++NumSocks;
	return 0;

} // end AcceptConnection()



//////////////////////////////////////////////////////////
// Receive data from a client (only in "reading" state)

int RecvData(int EventNum, int EventError)
{
	int nbytes;		// number of bytes written immediately

	// Check for error in event notification.
	if (EventError != 0) {
		printf("Read event error:  %d\n", EventError);
		CloseConnection(EventNum, 0);
		return -1;
	}

	// Receive data.  Note that we read just once and then will write
	// until this block of data is sent.  We receive the minimum of
	// the number of bytes available and the size of the buffer.
	if ((Socks[EventNum]->buflen = recv(Socks[EventNum]->sock, Socks[EventNum]->buf, BUFSIZE, 0)) == SOCKET_ERROR && WSAGetLastError() != WSAEWOULDBLOCK) {
		printf("Receive error: %d\n", WSAGetLastError());
		CloseConnection(EventNum, 0);
		return -1;
	}

	if (Socks[EventNum]->buflen > 0)
		printf("Status: received %d bytes (%d)\n", Socks[EventNum]->buflen, EventNum);

	// Assume that send() will not block and attempt to send data and
	// update buffer.
	Socks[EventNum]->bufptr = Socks[EventNum]->buf;
	do {
		nbytes = send(Socks[EventNum]->sock, Socks[EventNum]->bufptr, Socks[EventNum]->buflen, 0);
		if(nbytes > 0) {
			Socks[EventNum]->bufptr += nbytes;
			Socks[EventNum]->buflen -= nbytes;		
			printf("Status: sent %d bytes immediately (%d)\n", nbytes, EventNum);
		}
	} while (nbytes != SOCKET_ERROR && Socks[EventNum]->buflen > 0);

	
	// A socket error may be a real error, or it may be that the send would block.
	// If the send would block, then enter the ST_WRITING state and tell WinSock
	// to signal only WRITE and CLOSE events.
	if (nbytes == SOCKET_ERROR) {
		if (WSAGetLastError() != WSAEWOULDBLOCK) {
			printf("Write error:  %d\n", WSAGetLastError());
			CloseConnection(EventNum, 0);
			return -1;
		}
		else if (Socks[EventNum]->buflen > 0) {
			Socks[EventNum]->state = ST_WRITING;
			if (WSAEventSelect(Socks[EventNum]->sock, Events[EventNum], FD_WRITE | FD_CLOSE) == SOCKET_ERROR) {
				printf("Error reseting asynchronous event notification: %d\n", WSAGetLastError());
				CloseConnection(EventNum, 0);
				return -1;
			}
		}
	}

	return 0;

} // end RecvData()



///////////////////////////////////////////////////////////////////////////////
// Send data to a client (from state (only in "writing" or "closing" states)

int SendData(int EventNum, int EventError)
{
	int nbytes;		// number of bytes sent

	// Check for error in event notification
	if (EventError != 0) {
		printf("Write event error:  %d\n", EventError);
		Socks[EventNum]->state = ST_CLOSING;
		CloseConnection(EventNum, 0);
		return -1;
	}

	// If no data to send, just return
	if (Socks[EventNum]->buflen == 0)
		return 0;

	// Call send as long as there is data left to send or until the
	// send would block or encounters another error.
	do {
		nbytes = send(Socks[EventNum]->sock, Socks[EventNum]->bufptr, Socks[EventNum]->buflen, 0);
		if(nbytes > 0) {
			Socks[EventNum]->bufptr += nbytes;
			Socks[EventNum]->buflen -= nbytes;		
			printf("Status: sent %d bytes deferred (%d)\n", nbytes, EventNum);
		}
	} while (Socks[EventNum]->buflen > 0 && nbytes != SOCKET_ERROR);

	// If there is a socket error, it may be a real error or just that the
	// send call would block.  If it is a real error then close this
	// connection.  Otherwise just return and the SendData() routine will
	// be called again when the send will not block.
	if (nbytes == SOCKET_ERROR && WSAGetLastError() != WSAEWOULDBLOCK) {
		printf("Write error:  %d\n", WSAGetLastError());
		Socks[EventNum]->state = ST_CLOSING;
		CloseConnection(EventNum, 0);
		return -1;
	}
	
	// If all data is written and the connection is not closing, then we
	// are ready to attempt to receive data again.  Tell WinSock to check
	// for READ and CLOSE.  Also, call RecvData() to read pending data, if
	// any.  If closing, then just close connection since all data is now
	// written.
	if (Socks[EventNum]->buflen == 0) {
		if(Socks[EventNum]->state != ST_CLOSING) {
			Socks[EventNum]->state = ST_READING;
			if (WSAEventSelect(Socks[EventNum]->sock, Events[EventNum], FD_READ | FD_CLOSE) == SOCKET_ERROR) {
				printf("Error reseting asynchronous event notification: %d\n", WSAGetLastError());
				Socks[EventNum]->state = ST_CLOSING;
				CloseConnection(EventNum, 0);
				return -1;
			}
			RecvData(EventNum, 0);
		}
		else
			CloseConnection(EventNum, 0);
	}

	return 0;

} // end SendData()




////////////////////////////////////////////////////////////
// Close a client connection (potentially from any state)

int CloseConnection(int EventNum, int EventError)
{
	int i;

	// If still data to send and no error, allow writing, and change state.
	if (Socks[EventNum]->state == ST_WRITING && EventError == 0)
		Socks[EventNum]->state = ST_CLOSING;

	// Else, report error, if any, and free resources.
	else {
		if (EventError != 0)
			printf("Close event error:  %d\n", EventError);
		
		WSACloseEvent(Events[EventNum]);
		closesocket(Socks[EventNum]->sock);
		free(Socks[EventNum]);
	}

	// Delete this event and information pointer from arrays.
	for (i = EventNum + 1; i < NumSocks; ++i) {
		Events[i-1] = Events[i];
		Socks[i-1] = Socks[i];
	}
	--NumSocks;

	printf("Status: connection closed (%d)\n", EventNum);

	return 0;

} // end CloseConnection()




////////////////////////////////////////////////////////////////////////////
// Error exit routine as in Comer and Stevens, Vol. III (WinSock edition)

void errexit(const char *format, ...)
{
	va_list	args;

	va_start(args, format);
	vfprintf(stderr, format, args);
	va_end(args);
	WSACleanup();
	exit(1);

} // end errexit()



////////////////////////////////////////
// Display network events that are ready

void DisplayEvents(long events, int EventNum)
{
	printf("Event %d signalled for: ", EventNum);

	if (BITSET(FD_READ, events)) printf(" READ");
	if (BITSET(FD_WRITE, events)) printf(" WRITE");
	if (BITSET(FD_OOB, events)) printf(" OOB");
	if (BITSET(FD_ACCEPT, events)) printf(" ACCEPT");
	if (BITSET(FD_CONNECT, events)) printf(" CONNECT");
	if (BITSET(FD_CLOSE, events)) printf(" CLOSE");
	if (BITSET(FD_QOS, events)) printf(" QOS");
	if (BITSET(FD_GROUP_QOS, events)) printf(" GROUP_QOS");

	printf("\n");

} // end DisplayEvents()
