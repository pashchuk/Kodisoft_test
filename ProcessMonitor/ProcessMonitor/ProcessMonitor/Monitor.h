#ifndef __MONITOR_H_INCLUDED__
#define __MONITOR_H_INCLUDED__

//local include
#include<functional>
class Monitor
{
public:
	Monitor(int argc, char **argv);
	Monitor(Monitor &mon);
	~Monitor();
	void start();
	void restart();
	void stop();
	void suspend();
	void continuee();

private:
	int *p;
	int cmdCount;
	char **cmdArgs;
	//events
	std::function<void(Monitor&)> OnProcStart;
};

#endif // __MONITOR_H_INCLUDED__